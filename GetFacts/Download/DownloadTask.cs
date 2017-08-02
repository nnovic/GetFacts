using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Download
{
    [DebuggerDisplay("Uri = {Uri}")]
    public class DownloadTask: INotifyPropertyChanged
    {
        public const string TmpFileExtension = ".part";
        public const string BackupExtension = ".bak";

        private readonly object _lock_ = new object();
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler TaskFinished;
        public event EventHandler TaskStarted;

        private readonly Uri uri;
        private readonly Guid id;
        private readonly string defaultFileExtension;

        public DownloadTask(Uri uri, Guid id, string defaultFileExtension)
        {
            this.uri = uri;
            this.id = id;
            this.defaultFileExtension = defaultFileExtension;

            if (File.Exists(LocalFile))
            {
                _downloadStatus = DownloadStatus.Completed;
                Console.WriteLine("DownloadTask: {0} is there", uri.AbsoluteUri);
            }
        }

        public Uri Uri { get { return this.uri; } }
        public Guid Guid { get { return this.id; } }
        public string DefaultFileExtension { get { return this.defaultFileExtension; } }

        private void FirePropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(p);
                PropertyChanged(this, args);
            }
        }

        private void FireTaskFinished()
        {
            TaskFinished?.Invoke(this, EventArgs.Empty);
        }

        private void FireTaskStarted()
        {
            TaskStarted?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// The download task is finished if the file with
        /// corresponding Guid exists in download folder, or if
        /// the download has been aborted.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                return (Status==DownloadStatus.Completed)
                    || (Status == DownloadStatus.Aborted);
            }
        }


        

        

        /// <summary>
        /// Ranges from 1 (highest priority) to 3 (lowest priority)
        /// </summary>
        public int Priority
        {
            get
            {
                DownloadTypes.Categories cat = DownloadTypes.Guess(uri.AbsoluteUri);
                switch(cat)
                {
                    case DownloadTypes.Categories.Text:
                        return 1;

                    case DownloadTypes.Categories.Image:
                        return 2;

                    default:
                    case DownloadTypes.Categories.Video:
                        return 3;
                }
            }
        }

        #region download

        public enum DownloadStatus
        { 
            ReadyToStart,
            Connecting,
            Started,
            Aborted,
            Completed
        }

        private DownloadStatus _downloadStatus = DownloadStatus.ReadyToStart;
        private FileStream writeStream = null;
        private Stream readStream = null;
        private byte[] downloadBuffer = new byte[1024];
        private string downloadPath = null;
        private WebClient webClient = null;        
        private long expectedDownloadSize = -1;
        private long achievedDownloadSize = 0;

        public DownloadStatus Status
        {
            get
            {
                return _downloadStatus;
            }
            internal set
            {
                if (value != _downloadStatus)
                {
                    _downloadStatus = value;
                    FirePropertyChanged("Status");
                }
            }
        }

        /// <summary>
        /// Provoque le déclenchement de l'évènement TaskFinished
        /// si et seulement si Status==Completed.
        /// </summary>
        /// <returns>retourne 'true' si TaskFinished a été déclenché.
        /// retourne 'false' dans le cas contraire.</returns>
        public bool TriggerIfTaskFinished()
        {
            lock(_lock_)
            {
                if( Status== DownloadStatus.Completed)
                {
                    FireTaskFinished();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Reload()
        {
            lock(_lock_)
            {
                if( (Status==DownloadStatus.Connecting) || (Status==DownloadStatus.Started) )
                {
                    return false;
                }
                Status = DownloadStatus.ReadyToStart;
                return true;
            }
        }

        internal void Download()
        {
            lock (_lock_)
            {
                switch (Status)
                {
                    case DownloadStatus.ReadyToStart: OpenConnection(); break;
                    case DownloadStatus.Connecting: StartDownload(); break;
                    case DownloadStatus.Started: DownloadMore(); break;
                    default: break;
                }
            }
        }

        public int Progress
        {
            get
            {
                switch(Status)
                {
                    default:
                        return -1;

                    case DownloadStatus.Completed:
                        return 100;

                    case DownloadStatus.Started:
                        if (expectedDownloadSize > 0)
                        {
                            decimal p = (decimal)achievedDownloadSize / (decimal)expectedDownloadSize;
                            return (int)Math.Round(100 * p);
                        }
                       return -1;
                }
            }
        }

        /// <summary>
        /// Absolute path the to file on the computer
        /// that contains the downloaded data.
        /// </summary>
        public string LocalFile
        {
            get
            {
                string dirName = ConfigFactory.GetInstance().CacheDirectory;
                string fileName = id.ToString();              
                string path = Path.Combine(dirName, fileName);

                string extension = Path.GetExtension(uri.AbsoluteUri);
                if (string.IsNullOrEmpty(extension))
                    extension = DefaultFileExtension;
                if (string.IsNullOrEmpty(extension) == false)
                    path = Path.ChangeExtension(path, extension);

                if (path.EndsWith("."))
                    path = path.Remove(-1);

                return path;                
            }
        }

        internal void StopDownload()
        {
            try { readStream?.Dispose(); } catch { }
            try { webClient?.Dispose(); } catch { }
            try { writeStream?.Dispose(); } catch { }
            Status = DownloadStatus.Aborted;
        }

        private bool pendingAsyncOperation = false;

        private void OpenConnection()
        {
            if (pendingAsyncOperation == false)
            {
                try
                {
                    webClient = new WebClient();
                    webClient.OpenReadCompleted += WebClient_OpenReadCompleted;
                    pendingAsyncOperation = true;
                    webClient.OpenReadAsync(uri);
                    Status = DownloadStatus.Connecting;
                }
                catch
                {
                    pendingAsyncOperation = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Ajoute une notification dans NotificationSystem en cas
        /// d'erreur durant l'obtention du flux d'entrée.</remarks>
        /// <seealso cref="NotificationKeys.CannotOpenConnection"/>
        private void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            lock(_lock_)
            {
                var notification = new NotificationSystem.Notification(this,
                                (int)NotificationKeys.CannotOpenConnection)
                {
                    Title = Uri.ToString(),
                    Description = "Connection cannot be established."
                };

                try
                {
                    readStream = e.Result;
                    NotificationSystem.GetInstance().Remove(notification);
                }
                catch
                {
                    readStream = null;
                    NotificationSystem.GetInstance().Add(notification);
                }
                finally
                {
                    pendingAsyncOperation = false;
                }
            }
        }

        private void StartDownload()
        {
            if(pendingAsyncOperation==true)
            {
                return;
            }

            startDownloadCounter++;

            // the following condition might occur
            // if OpenReadAsync failed:
            if(readStream==null)
            {
                AbortDownload();
                return;
            }
            
            expectedDownloadSize = -1;
            achievedDownloadSize = 0;
            string contentLengthAsString = webClient.ResponseHeaders[HttpResponseHeader.ContentLength];
            if (string.IsNullOrEmpty(contentLengthAsString) == false)
            {
                if (long.TryParse(contentLengthAsString, out expectedDownloadSize) == false)
                {
                    expectedDownloadSize = -1;
                }
            }

            downloadPath = Path.ChangeExtension(LocalFile, TmpFileExtension);
            writeStream = File.OpenWrite(downloadPath);
            Status = DownloadStatus.Started;
            FireTaskStarted();
            Console.WriteLine("DownloadTask: start {0}", uri.AbsoluteUri);
        }        

        private void DownloadMore()
        {
            try
            {
                int read = readStream.Read(downloadBuffer, 0, 1024);
                if (read == 0)
                {
                    EndOfDownload();
                    return;
                }

                writeStream.Write(downloadBuffer, 0, read);

                int previous = Progress;
                achievedDownloadSize += read;
                if (Progress != previous)
                {
                    FirePropertyChanged("Progress");
                }
            }
            catch
            {
                AbortDownload();
            }
        }

        private void TerminateStreams()
        {
            try { writeStream?.Dispose(); } catch { } finally { writeStream = null; }
            try { readStream?.Dispose(); } catch { } finally { readStream = null; }
            try { webClient?.Dispose(); } catch { } finally { webClient = null; }
            expectedDownloadSize = -1;
            achievedDownloadSize = 0;
        }

        private void AbortDownload()
        {
            TerminateStreams();
            if (downloadPath != null)
            {
                try { File.Delete(downloadPath); } catch { }
                downloadPath = null;
            }
            Status = DownloadStatus.Aborted;
            FireTaskFinished();
            Console.WriteLine("DownloadTask: abort {0}", uri.AbsoluteUri);
        }

        private void EndOfDownload()
        {
            string backupPath = Path.ChangeExtension(LocalFile, BackupExtension);
            TerminateStreams();
            if (File.Exists(LocalFile))
            {
                File.Replace(downloadPath, LocalFile, backupPath);
            }
            else
            {
                File.Move(downloadPath, LocalFile);
            }
            downloadPath = null;
            Status = DownloadStatus.Completed;
            FireTaskFinished();
            Console.WriteLine("DownloadTask: finished {0}", uri.AbsoluteUri);
        }

        #endregion


        #region counters

        private long startDownloadCounter = 0;

        /// <summary>
        /// Retourne le nombre de fois où cette tache
        /// a été lancée depuis sa création.
        /// </summary>
        public long StartCounter
        {
            get { return startDownloadCounter; }
        }

        #endregion

        /// <summary>
        /// Enumération des clés que cette classe utilise
        /// pour insérer/supprimer des notifications dans
        /// NotificationSystem.
        /// </summary>
        enum NotificationKeys
        {
            /// <summary>
            /// Indique que la connexion vers le site internet
            /// n'a pu être établie. Mauvais URL ? Site en panne ?
            /// </summary>
            CannotOpenConnection
        }
    }
}
