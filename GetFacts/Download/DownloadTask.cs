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
        private readonly string downloadPath = null;

        public DownloadTask(Uri uri, Guid id, string defaultFileExtension)
        {
            this.uri = uri;
            this.id = id;
            this.defaultFileExtension = defaultFileExtension;
            this.downloadPath = Path.ChangeExtension(LocalFile, TmpFileExtension);

            if (File.Exists(LocalFile))
            {
                _downloadStatus = DownloadStatus.Completed;
            }
        }

        /// <summary>
        /// Destruction de cet objet:
        /// - s'assurer que toutes les notifications poussées dans
        ///   NotificationSystem par cet objet soient supprimées.
        /// </summary>
        ~DownloadTask()
        {
            NotificationSystem.GetInstance().RemoveAll(this);
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
                // remove parameters at the end of the Uri
                // (anything beyond the '?' mark)
                string strippedAbsoluteUri = uri.AbsoluteUri;
                int markIndex = uri.AbsoluteUri.LastIndexOf('?');
                if (markIndex != -1)
                {
                    strippedAbsoluteUri = uri.AbsoluteUri.Remove(markIndex);
                }

                DownloadTypes.Categories cat = DownloadTypes.Guess(strippedAbsoluteUri);
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
        private HttpWebRequest webRequest = null;
        private HttpWebResponse webResponse = null;
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

                // remove parameters at the end of the Uri
                // (anything beyond the '?' mark)
                string strippedAbsoluteUri = uri.AbsoluteUri;
                int markIndex = uri.AbsoluteUri.LastIndexOf('?');
                if (markIndex != -1)
                {
                    strippedAbsoluteUri = uri.AbsoluteUri.Remove(markIndex);
                }

                string extension = Path.GetExtension(strippedAbsoluteUri);
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
            try { webResponse?.Dispose(); } catch { }
            try { writeStream?.Dispose(); } catch { }
            webRequest = null;
            Status = DownloadStatus.Aborted;
        }

        private bool pendingAsyncOperation = false;

        private void OpenConnection()
        {
            if (pendingAsyncOperation == false)
            {
                try
                {
                    pendingAsyncOperation = true;
                    webRequest = WebRequest.CreateHttp(uri);
                    Task<WebResponse> task = webRequest.GetResponseAsync();
                    task.ContinueWith(OpenResponseStream);
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
        /// <param name="task"></param>
        /// <remarks>Ajoute une notification dans NotificationSystem en cas
        /// d'erreur durant l'obtention du flux d'entrée.</remarks>
        /// <seealso cref="NotificationKeys.CannotOpenConnection"/>
        private void OpenResponseStream(Task<WebResponse> task)
        {
            lock (_lock_)
            {
                var notification = new NotificationSystem.Notification(this,
                                (int)NotificationKeys.CannotOpenConnection)
                {
                    Title = Uri.ToString(),
                    Description = "Connection cannot be established."
                };


                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        pendingAsyncOperation = false;
                        webResponse = (HttpWebResponse)task.Result;
                        readStream = webResponse.GetResponseStream();
                        break;

                    default:
                        break;

                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        pendingAsyncOperation = false;
                        webResponse = null;
                        readStream = null;
                        NotificationSystem.GetInstance().Add(notification);
                        break;
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
            
            achievedDownloadSize = 0;
            expectedDownloadSize = webResponse.ContentLength;
            
            writeStream = File.OpenWrite(downloadPath);
            Status = DownloadStatus.Started;
            FireTaskStarted();
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
            try { webResponse?.Dispose(); } catch { } finally { webResponse = null; }
            webRequest = null;
            expectedDownloadSize = -1;
            achievedDownloadSize = 0;
        }

        private void AbortDownload()
        {
            TerminateStreams();
            Status = DownloadStatus.Aborted;
            FireTaskFinished();
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
            Status = DownloadStatus.Completed;
            FireTaskFinished();
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
