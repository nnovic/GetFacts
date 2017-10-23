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
    public class DownloadManager
    {
        #region singleton 

        protected static DownloadManager uniqueInstance = null;
        private static readonly object _lock_ = new object();

        /// <summary>
        /// Retourne l'instance unique du DownloadManager.
        /// L'objet sera instancié s'il n'existe pas encore, 
        /// la configuration sera lue depuis le fichier-listing
        /// des téléchargements et le thread de téléchargement sera
        /// démarré.
        /// </summary>
        /// <returns></returns>
        public static DownloadManager GetInstance()
        {
            lock(_lock_)
            {
                if (uniqueInstance == null)
                {
                    uniqueInstance = new DownloadManager();
                    uniqueInstance.Initialize();
                }
                return uniqueInstance;
            }
        }

        #endregion

        private Regex downloadListPattern = null;
        private ObservableCollection<DownloadTask> downloads = new ObservableCollection<DownloadTask>();

        protected DownloadManager()
        {
        }

        private void Initialize()
        {
            LoadTasksFromFile();
            StartDownloadQueue();
        }

        /// <summary>
        /// Construit le Regex qui sera utilisé pour
        /// analyser les lignes du fichier "dowloads.lst"
        /// </summary>
        /// <remarks>
        /// Le Regex ainsi obtenu est stocké pour 
        /// les prochains appels à cette propriété.
        /// </remarks>
        private Regex DownloadListPattern
        {
            get
            {
                if(downloadListPattern==null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("\"([^\"]+)\""); // any block of text that start and ends with quotes
                    sb.Append("\\s+"); // any number of white spaces
                    sb.Append("\\{([^\\}]+)\\}"); // any block of text that starts and ends with curly braces
                    sb.Append("(?:\\s(\\.[\\w\\d]+))?"); // optional non-capturing group
                    downloadListPattern = new Regex(sb.ToString());
                }
                return downloadListPattern;
            }
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
                string listFile = ConfigManager.GetInstance().DownloadsList;
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
                string path = ConfigManager.GetInstance().DownloadsList;
                string[] entries = File.ReadAllLines(path);
                List<string> list = new List<string>(entries);
                foreach(string d in removeList)
                {
                    Remove(d);
                }
                SaveTasksToFile();
            }
        }

        /// <summary>
        /// Si 'true' (par défaut), le DownloadManager va lire le contenu
        /// du fichier "DownloadsFile" pour connaître la liste
        /// des DownloadTask en cours, et va sauvegarder cette liste
        /// à chaque ajout/suppression d'une DownloadTask.        
        /// </summary>
        /// <remarks>/// Doit être overridé si on doit avoir 'false'.</remarks>
        protected virtual bool DownloadsFileSupported
        {
            get { return true; }
        }


        /// <summary>
        /// Saves the list of currently known tasks into the 'DownloadsList' file.
        /// Each line of text in the file is a task.
        /// Each task is described by three components: 
        /// - the URL from which the file has been downloaded [Mandatory]
        /// - the GUID of the task (which also is corresponding local file's name without extension) [Mandatory]
        /// - the extension of the local file name, which is only required if its missing from URL [Optional]
        /// </summary>
        private void SaveTasksToFile()
        {
            if (DownloadsFileSupported)
            {
                string path = ConfigManager.GetInstance().DownloadsList;
                List<string> entries = new List<string>();
                foreach (DownloadTask task in downloads)
                {
                    string uri = task.Uri.AbsoluteUri;
                    StringBuilder sb = new StringBuilder();
                    
                    // URL (mandatory)
                    sb.AppendFormat("\"{0}\"", uri).Append(" ");

                    // Guid (mandatory)
                    sb.Append("{").Append(task.Guid).Append("}");


                    if( Path.HasExtension(uri)==false )
                    {
                        string ext = Path.GetExtension(task.LocalFile);
                        sb.AppendFormat(" {0}", ext);
                    }

                    entries.Add(sb.ToString());
                }
                File.WriteAllLines(path, entries.ToArray());
            }
        }

        /// <summary>
        /// Lit le contenu de DownloadsFile, et l'utilise pour
        /// créer des DownloadTasks qui sont ajoutés à la liste
        /// du présent DownloadManager.
        /// Cette méthode ne fait rien si DownloadsFileSupported vaut 'false'.
        /// </summary>
        /// <remarks>method indirectly called from GetInstance() , which is synchronized with the "_lock_" object.</remarks>
        private void LoadTasksFromFile()
        {
            try
            {
                if (DownloadsFileSupported)
                {
                    string path = ConfigManager.GetInstance().DownloadsList;
                    string[] entries = File.ReadAllLines(path);
                    foreach (string entry in entries)
                    {
                        DownloadTask dt = CreateTaskFromString(entry);
                        downloads.Add(dt);
                        Console.WriteLine("DownloadManager: task {0} added to list", dt.Uri.AbsoluteUri);
                    }
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
        /// Analyse une ligne qui provient du fichier
        /// "downloads.lst" grâce à l'expression régulière
        /// donnée par la propriété DownloadListPattern.
        /// Les informations ainsi obtenues permettent de 
        /// créer une DownloadTask, qui est ensuite retournée.
        /// </summary>
        /// <param name="text">une ligne de "downloads.lst" à analyser</param>
        /// <returns>un objet DownloadTask correspondant aux infos de la ligne passée en paramètre. La valeur null
        /// sera retournée si l'analyse échoue.</returns>
        /// <remarks>method called from the constructor, which is synchronized with the "_lock_" object.</remarks>
        private DownloadTask CreateTaskFromString(string text)
        {
            Match m = DownloadListPattern.Match(text);
            if( m.Success == false )
            {
                return null;
            }

            string url = m.Groups[1].Value;
            string guidAsString = m.Groups[2].Value;
            string extension = (m.Groups.Count>=4)? m.Groups[3].Value:null;


            Guid guid = Guid.Parse(guidAsString);
            Uri uri = new Uri(url, UriKind.Absolute);
            DownloadTask task = new DownloadTask(uri, guid, extension);

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

        /// <summary>
        /// Retourne un objet DownloadTask associé à
        /// l'Uri passé en paramètre. S'il en existe
        /// déjà un dans la liste des tâches, c'est cet
        /// instance qui sera retournée. Sinon, un nouveau
        /// DowloadTask est créé, ajouté à la liste des tâches,
        /// et retourné.
        /// </summary>
        /// <param name="uri">L'Uri pour lequel on veut obtenir
        /// un objet DownloadTask</param>
        /// <param name="defaultFileExtension">Précise l'extension de fichier qu'il faudrait associé à la ressource pointée par l'Uri. Doit être
        /// précisé si l'Uri ne fait pas apparaître explicitement une extension de fichier. Peut valoir null si l'Uri indique
        /// explicitement l'extension du fichier.</param>
        /// <returns>La DownloadTask associé à uri.</returns>
        /// <example>
        /// FindOrQueue( new Uri("http://www.google.fr/"), ".html");
        /// FindOrQueue( new Uri("http://www.site.com/index.htm", null);
        /// </example>
        public DownloadTask FindOrQueue(Uri uri, string defaultFileExtension)
        {
            DownloadTask output = null;
            lock(_lock_)
            {
                output = Find(uri);
                if(output==null)
                {
                    output = Queue(uri, defaultFileExtension);
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
        }

        public DownloadTask Create(Uri uri, string defaultFileExtension)
        {
            DownloadTask newTask = null;
            lock(_lock_)
            {
                if (Find(uri) != null)
                {
                    throw new ArgumentException("already in queue");
                }

                Guid newId = GenerateUniqueId();
                newTask = new DownloadTask(uri, newId, defaultFileExtension);
            }
            return newTask;
        }

        public DownloadTask Queue(Uri uri, string defaultFileExtension)
        {
            DownloadTask newTask = null;

            lock(_lock_)
            {
                newTask = Create(uri, defaultFileExtension);
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
            dThread = new Thread(DownloadLoop)
            {
                Name = "DownloadLoop"
            };
            isRunning = true;
            dThread.Start();
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
        }

        #endregion
    }
}
