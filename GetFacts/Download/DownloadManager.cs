using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GetFacts.Download
{
    class DownloadManager
    {
        #region singleton 

        private static DownloadManager uniqueInstance = null;
        private static readonly object _lock_ = new object();

        public static DownloadManager GetInstance()
        {
            lock(_lock_)
            {
                if (uniqueInstance == null)
                {
                    uniqueInstance = new DownloadManager();
                    Console.WriteLine("DownloadManager: instance created");
                }
                return uniqueInstance;
            }
        }

        #endregion

        private Regex downloadListPattern = new Regex("\"([^\"]+)\"\\s+\\{([^\\}]+)\\}");
        private ObservableCollection<DownloadTask> downloads = new ObservableCollection<DownloadTask>();

        private DownloadManager()
        {
            LoadTasksFromFile();
            StartDownloadQueue();
        }


        public ISet<string> GetAllUrls()
        {
            HashSet<string> output = new HashSet<string>();
            lock (_lock_)
            {
                foreach (DownloadTask task in downloads)
                {
                    output.Add(task.Uri.AbsoluteUri);
                }
            }
            return output;
        }

        public ISet<string> GetAllFilesInUse()
        {
            HashSet<string> output = new HashSet<string>();
            lock (_lock_)
            {
                string listFile = DownloadsFile;
                output.Add(listFile);
                output.Add(Path.ChangeExtension(listFile, DownloadTask.BackupExtension));
                output.Add(Path.ChangeExtension(listFile, DownloadTask.TmpFileExtension));

                foreach (DownloadTask task in downloads)
                {
                    output.Add(task.LocalFile);
                    output.Add(Path.ChangeExtension(task.LocalFile, DownloadTask.BackupExtension));
                    output.Add(Path.ChangeExtension(task.LocalFile, DownloadTask.TmpFileExtension));
                }
            }
            return output;
        }

        public void DeleteFiles(IEnumerable<string> removeList)
        {
            if (removeList.Any() == false)
                return;

            foreach(string s in removeList)
            {
                if( s== "C:\\985bd681-3e0b-4689-960e-a2f1b761765f")
                {

                }
                try
                {
                    File.Delete(s);
                }
                catch
                {
                }
            }
        }

        public void RemoveTasks(IEnumerable<string> removeList)
        {
            if (removeList.Any() == false)
                return;

            lock(_lock_)
            {
                string path = DownloadsFile;
                string[] entries = File.ReadAllLines(path);
                List<string> list = new List<string>(entries);
                foreach(string d in removeList)
                {
                    Remove(d);
                }
                SaveTasksToFile();
            }
        }


        private string DownloadsFile
        {
            get
            {
                string dirName = ConfigurationManager.AppSettings.Get("DownloadDirectory");
                string fileName = ConfigurationManager.AppSettings.Get("DownloadList");
                string path = Path.Combine(dirName, fileName);
                return path;
            }
        }

        private void SaveTasksToFile()
        {
            string path = DownloadsFile;
            List<string> entries = new List<string>();
            foreach (DownloadTask task in downloads)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("\"{0}\"", task.Uri.AbsoluteUri).Append(" ");
                sb.Append("{").Append(task.Guid).Append("}");
                entries.Add(sb.ToString());
            }
            File.WriteAllLines(path, entries.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>method called from the constructor, which is synchronized with the "_lock_" object.</remarks>
        private void LoadTasksFromFile()
        {
            try
            {
                string path = DownloadsFile;
                string[] entries = File.ReadAllLines(path);
                foreach (string entry in entries)
                {
                    DownloadTask dt = CreateTaskFromString(entry);
                    downloads.Add(dt);
                    Console.WriteLine("DownloadManager: task {0} added to list", dt.Uri.AbsoluteUri);
                }
            }
            catch (FileNotFoundException)
            {
                // pas grave
            }
            catch(DirectoryNotFoundException)
            {
                // pas grave
            }
        }

        // "http:\\blabla\toto.ext" {Guid}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>method called from the constructor, which is synchronized with the "_lock_" object.</remarks>
        private DownloadTask CreateTaskFromString(string text)
        {
            Match m = downloadListPattern.Match(text);
            if( m.Success == false )
            {
                return null;
            }

            string url = m.Groups[1].Value;
            string guidAsString = m.Groups[2].Value;

            Guid guid = Guid.Parse(guidAsString);
            Uri uri = new Uri(url, UriKind.Absolute);
            DownloadTask task = new DownloadTask(uri, guid);

            return task;
        }

        public bool Exists(DownloadTask t)
        {
            lock(_lock_)
            {
                foreach (DownloadTask task in downloads)
                {
                    if (task == t) return true;
                    //if (string.Compare(task.Url, t.Url, true) == 0) return true;
                    if (task.Uri == t.Uri) return true;
                    if (task.Guid == t.Guid) return true;
                }
            }
            return false;
        }

        public DownloadTask FindOrQueue(Uri uri)
        {
            DownloadTask output = null;
            lock(_lock_)
            {
                output = Find(uri);
                if(output==null)
                {
                    output = Queue(uri);
                }
            }
            return output;
        }

        public DownloadTask Find(Uri uri)
        {
            lock (_lock_)
            {
                foreach (DownloadTask task in downloads)
                {
                    //if (string.Compare(task.Url, url, true) == 0)
                    if (task.Uri==uri)
                    {
                        return task;
                    }
                }
            }

            return null;
        }

        private Guid GenerateUniqueId()
        {
            return Guid.NewGuid();
        }

        public void Queue(DownloadTask task)
        {
            lock (_lock_)
            {
                if (Exists(task) ==true)
                {
                    throw new ArgumentException("already in queue");
                }
                downloads.Add(task);
                SaveTasksToFile();
                Console.WriteLine("DownloadManager: task {0} added to list", task.Uri.AbsoluteUri);
            }

            WakeUpDownloadQueue();
        }

        private void Remove(string url)
        {
            Remove(new Uri(url, UriKind.Absolute));
        }

        private void Remove(Uri uri)
        {
            DownloadTask task = Find(uri);           
            if (task==null)
            {
                throw new ArgumentException("task does not exist");
            }
            downloads.Remove(task);
            task.StopDownload();
            Console.WriteLine("DownloadManager: task {0} removed from list", task.Uri.AbsoluteUri);
        }

        public DownloadTask Create(Uri uri)
        {
            DownloadTask newTask = null;
            lock(_lock_)
            {
                if (Find(uri) != null)
                {
                    throw new ArgumentException("already in queue");
                }

                Guid newId = GenerateUniqueId();
                newTask = new DownloadTask(uri, newId);
            }
            return newTask;
        }

        public DownloadTask Queue(Uri uri)
        {
            DownloadTask newTask = null;

            lock(_lock_)
            {
                newTask = Create(uri);
                Queue(newTask);
            }

            return newTask;
        }

        public ObservableCollection<DownloadTask> Items
        {
            get { return downloads; }
        }

        #region download queue

        private Thread dThread = null;
        private bool isRunning = false;

        private void StartDownloadQueue()
        {
            dThread = new Thread(DownloadLoop);
            dThread.Name = "DownloadLoop";
            isRunning = true;
            dThread.Start();
            Console.WriteLine("DownloadManager: thread started");
        }


        public void Stop()
        {
            isRunning = false;
            dThread.Interrupt();
        }

        private void WakeUpDownloadQueue()
        {
        }

        private void DownloadLoop()
        {
            DownloadScheduler scheduler = null;

            while (isRunning)
            {
                try
                {
                    // Mécanisme de sélection de la prochaine tâche
                    // à activer :

                    DownloadTask task = null;

                    lock (_lock_)
                    {
                        if (scheduler == null)
                        {
                            scheduler = new DownloadScheduler(downloads);
                        }
                        task = scheduler.Next();
                    }

                    if (task != null)
                    {
                        task.Download();
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            lock(_lock_)
            {
                foreach (DownloadTask dt in downloads)
                {
                    try
                    {
                        dt.StopDownload();
                    }
                    catch
                    {
                    }
                }
            }

            Console.WriteLine("DownloadManager: thread stopped");
        }

        #endregion
    }
}
