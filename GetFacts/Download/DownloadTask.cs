using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Download
{
    public class DownloadTask: INotifyPropertyChanged
    {
        public const string TmpFileExtension = ".part";
        public const string BackupExtension = ".bak";

        private readonly object _lock_ = new object();
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler TaskFinished;

        private readonly Uri uri;
        private readonly Guid id;

        public DownloadTask(Uri uri, Guid id)
        {
            this.uri = uri;
            this.id = id;

            if (File.Exists(LocalFile))
            {
                _downloadStatus = DownloadStatus.Completed;
                Console.WriteLine("DownloadTask: {0} is there", uri.AbsoluteUri);
            }
        }

        public Uri Uri { get { return this.uri; } }
        public Guid Guid { get { return this.id; } }


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

        internal void TriggerIfTaskFinished()
        {
            lock(_lock_)
            {
                if( Status== DownloadStatus.Completed)
                {
                    FireTaskFinished();
                }
            }
        }

        internal bool Reload()
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

        public string LocalFile
        {
            get
            {
                string dirName = ConfigurationManager.AppSettings.Get("DownloadDirectory");
                string fileName = id.ToString();
                string extension = Path.GetExtension(uri.AbsoluteUri);

                //string path = Path.ChangeExtension( Path.Combine(dirName, fileName), extension);
                string path = Path.Combine(dirName, fileName);
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
            /*if(File.Exists(LocalFile) )
            {
                Status = DownloadStatus.Completed;
                FireTaskFinished();
                return;
            }*/

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

        private void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            lock(_lock_)
            {
                try
                {
                    readStream = e.Result;
                }
                catch
                {
                    readStream = null;
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
    }
}
