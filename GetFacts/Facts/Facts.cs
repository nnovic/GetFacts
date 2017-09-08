using GetFacts.Download;
using GetFacts.Parse;
using GetFacts.Render;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Facts
{
    public class Facts
    {
        protected static Facts uniqueInstance = null;
        private readonly static object _lock_ = new object();

        public static Facts GetInstance()
        {
            lock (_lock_)
            {
                if (uniqueInstance == null)
                {
                    string cfgFile = ConfigFactory.GetInstance().ConfigFile;
                    uniqueInstance = new Facts(cfgFile);
                }
                return uniqueInstance;
            }
        }

        //---------------------------------------------------------------------

        protected Facts(string path)
        {
            LoadConfiguration(path);
        }

        private void LoadConfiguration(string path)
        {
            List<PageConfig> listConfigs = ConfigFactory.GetInstance().LoadConfig(path);
            IEnumerable<PageConfig> orderedList = listConfigs;

            if(ShufflePages)
            {
                Random rnd = new Random();
                orderedList = listConfigs.OrderBy<PageConfig, int>((item) => rnd.Next());
            }

            foreach (PageConfig config in orderedList)
            {
                try
                {
                    Page p = new Page(config);
                    AddPage(p);
                }
                catch
                {
                    // ne pas planter le système si une page
                    // n'a pas pu être ajoutée.
                }
            }
        }

        private readonly Hashtable downloadTasks = new Hashtable();

        public void Initialize()
        {
            try
            {
                AbstractInfo.NewStatusForNewInstance = false;
                foreach (Page p in pages)
                {                    
                    DownloadTask task = DownloadManager.GetInstance().FindOrQueue(p.BaseUri, p.DefaultFileExtension);
                    downloadTasks.Add(task, p);

                    // Ne pas activer le téléchargement
                    // périodique de la page si
                    // elle n'est pas activée:
                    if (p.Enabled == true)
                    {
                        task.TaskStarted += Task_TaskStarted;
                        task.TaskFinished += DownloadTask_TaskFinished;
                        task.TriggerIfTaskFinished();
                    }
                }
            }
            finally
            {
                AbstractInfo.NewStatusForNewInstance = true;
            }
        }

        private void Task_TaskStarted(object sender, EventArgs e)
        {
            IncrementPageRefreshCount();
        }

        private void DownloadTask_TaskFinished(object sender, EventArgs e)
        {
            DecrementPageRefreshCount();
            DownloadTask task = (DownloadTask)sender;
            Page p = downloadTasks[task] as Page;
            int delay = p.RecoverDelay * 1000;

            if (task.Status == DownloadTask.DownloadStatus.Completed)
            {
                p.Update(task.LocalFile);
                if(task.StartCounter>0)
                {
                    delay = p.RefreshDelay * 1000;
                }
            }

            Task.Run(async delegate
            {
                await Task.Delay(delay);
                task.Reload();
            });
        }

        #region Curseurs

        private int pageIndex = -1;
        private int sectionIndex = -1;
        private int articleIndex = -1;

        private Page CurrentPage()
        {
            if ((pageIndex >= 0) && (pageIndex < pages.Count))
            {
                return pages[pageIndex];
            }
            return null;
        }

        /// <summary>
        /// Incrémente l'index de page et retourne l'objet Page 
        /// correspondant. Si la dernière page a été atteinte,
        /// NextPage() réinitialise l'index et retourne la première page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Réinitialise l'index de section et l'index d'article</remarks>
        private Page NextPage()
        {
            pageIndex++;
            sectionIndex = -1;
            articleIndex = -1;
            if (pageIndex >= pages.Count)
            {
                pageIndex = 0;
            }
            return pages[pageIndex];
        }

        private Section CurrentSection()
        {
            Page p = CurrentPage();
            if (p == null) return null;

            if ((sectionIndex >= 0) && (sectionIndex < p.SectionsCount))
            {
                return p.GetSection(sectionIndex);
            }
            return null;
        }

        /// <summary>
        /// Incrémente l'index de section et retourne
        /// la section correspondante dans la Page
        /// fournie en argument. Si le nouvel index
        /// est en-dehors des limites, retourne null.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <remarks>Réinitialise l'index d'article</remarks>
        private Section NextSection(Page p)
        {
            if (p == null) throw new ArgumentNullException();
            sectionIndex++;
            articleIndex = -1;
            if (sectionIndex >= p.SectionsCount)
            {
                return null;
            }
            return p.GetSection(sectionIndex);
        }

        public Article More()
        {
            Section s = CurrentSection();
            return More(s);
        }

        /// <summary>
        /// Increment the article index and return the
        /// corresponding article in the provided Section.
        /// The method will skip articles that do not provided any
        /// displayable information.
        /// </summary>
        /// <param name="s">the Section from which articles are obtained</param>
        /// <returns>The next article in the provided Section. returns null if a valid Article cannot be obtained from the Section, whatever the reason is.</returns>
        /// <exception cref="ArgumentNullException">provided Section is null</exception>
        private Article More(Section s)
        {
            if (s == null) throw new ArgumentNullException();

            Article output = null;

            do
            {
                if ((articleIndex + 1) >= s.ArticlesCount)
                {
                    return null;
                }

                articleIndex++;
                output = s.GetArticle((int)articleIndex);

            } while (output.HasContent == false);


            return output;
        }

        /// <summary>
        /// Fait appel à More() pour obtenir l'article suivant
        /// dans la section en cours de la page courante.
        /// En cas d'échec,
        /// 
        /// </summary>
        /// <param name="pageChanged"></param>
        /// <param name="sectionChanged"></param>
        /// <returns></returns>
        private Article Next(out bool pageChanged, out bool sectionChanged)
        {
            Page startingPage, currentPage;
            Section startingSection, currentSection;
            Article output;

            pageChanged = false;
            sectionChanged = false;

            // Essaye d'obtenir l'article suivant
            // de la section en cours dans la page courante,
            // et le retourne en cas de succès.

            startingPage = currentPage = CurrentPage();
            if (currentPage == null)
            {
                startingSection = currentSection = null;
                output = null;
            }
            else
            {
                startingSection = currentSection = CurrentSection();
                output = More(currentSection);

                if (output != null)
                    return output;
            }

            for (int tryPage = 0; tryPage <= pages.Count; tryPage++)
            {
                // Trouver la prochaine section de la page
                // courante qui contiendrait au moins un article
                // affichable.
                if (currentPage != null)
                {
                    // section suivante
                    currentSection = NextSection(currentPage);
                    if (currentSection != null)
                    {

                        // premier article non-vide de la section
                        output = More(currentSection);
                        if (output != null)
                        {
                            sectionChanged = (currentSection != startingSection);
                            pageChanged = (currentPage != startingPage);
                            return output;
                        }

                    }
                }
                // Trouver la prochain page
                currentPage = NextPage();
            }

            return null;
        }

        #endregion


        #region Pages


        public List<System.Windows.Controls.Page> CreateNextPages()
        {
            List<System.Windows.Controls.Page> output = new List<System.Windows.Controls.Page>();
            Page p = null;
            Section s = null;
            Article a1, a2, a3 = null;
            bool pageChanged = false;
            bool sectionChanged = false;

            lock (_lock_)
            {
                a1 = Next(out pageChanged, out sectionChanged);
                if (a1 == null)
                {
                    return null;
                }

                // if (pageChanged || sectionChanged)
                // {
                p = CurrentPage();
                s = CurrentSection();
                //}

                a2 = More();

                a3 = More();
            }


            StringBuilder sb = new StringBuilder();

            if (pageChanged || sectionChanged)
            {
                sb.AppendLine("===========================================================================");
                sb.AppendFormat("   Page={0}", p.Title).AppendLine();
                sb.AppendFormat("   Section={0}", s.Title).AppendLine();
                sb.AppendLine("===========================================================================").AppendLine();
                System.Windows.Controls.Page pageIntercalaire = new SpacerPage(p, s);
                output.Add(pageIntercalaire);
            }

            sb.AppendLine("---------------------------------------------------------------------------");
            if (a1 != null)
            {
                sb.AppendFormat("   Title={0}", a1.Title).AppendLine();
            }
            if (a2 != null)
            {
                sb.AppendFormat("   Title={0}", a2.Title).AppendLine();
            }
            if (a3 != null)
            {
                sb.AppendFormat("   Title={0}", a3.Title).AppendLine();
            }
            sb.AppendLine("---------------------------------------------------------------------------").AppendLine();

            TriPage tp = new TriPage(p, s);
            if (a1 != null) tp.AddArticle(a1);
            if (a2 != null) tp.AddArticle(a2);
            if (a3 != null) tp.AddArticle(a3);
            output.Add(tp);


            Console.Write(sb.ToString());


            return output;
        }


        protected readonly List<Page> pages = new List<Page>();

        void AddPage(Page p)
        {
            lock (_lock_)
            {
                if (pages.Contains(p) == true)
                {
                    throw new ArgumentException();
                }

                pages.Add(p);
            }
        }


        public bool ShufflePages
        {
            get { return true; }
        }

        #endregion


        #region Page events

        private readonly object pageRefreshEventLock = new object();
        private int pageRefreshEventCount = 0;

        private void IncrementPageRefreshCount()
        {
            lock(pageRefreshEventLock)
            {
                if(pageRefreshEventCount==0)
                {
                    OnPageRefreshBegins();
                }
                pageRefreshEventCount++;
            }
        }

        private void DecrementPageRefreshCount()
        {
            lock(pageRefreshEventLock)
            {
                if(pageRefreshEventCount>0)
                {
                    pageRefreshEventCount--;
                    if(pageRefreshEventCount==0)
                    {
                        OnPageRefreshEnds();
                    }
                }
            }
        }

        public class PageRefreshEventArgs : EventArgs
        {
            private readonly bool begins;
            public PageRefreshEventArgs(bool begins)
            {
                this.begins = begins;
            }
            public bool Begins
            {
                get { return begins; }
            }
            public bool Ends
            {
                get { return !begins; }
            }
        }

        public event EventHandler<PageRefreshEventArgs> PageRefreshEvent;

        protected void OnPageRefreshBegins()
        {
            PageRefreshEventArgs prea = new PageRefreshEventArgs(true);
            PageRefreshEvent?.Invoke(this, prea);
        }

        protected void OnPageRefreshEnds()
        {
            PageRefreshEventArgs prea = new PageRefreshEventArgs(false);
            PageRefreshEvent?.Invoke(this, prea);
        }
        #endregion

        public ISet<string> GetAllUrls()
        {
            HashSet<string> output = new HashSet<string>();
            lock (_lock_)
            {
                foreach (Page p in pages)
                {
                    if (!string.IsNullOrEmpty(p.Url)) output.Add(p.Url);
                    if (!string.IsNullOrEmpty(p.IconUrl)) output.Add(p.IconUri.AbsoluteUri);
                    if (!string.IsNullOrEmpty(p.MediaUrl)) output.Add(p.MediaUri.AbsoluteUri);

                    for (int sectionIndex = 0; sectionIndex < p.SectionsCount; sectionIndex++)
                    {
                        Section s = p.GetSection(sectionIndex);
                        if (!string.IsNullOrEmpty(s.IconUrl)) output.Add(s.IconUri.AbsoluteUri);
                        if (!string.IsNullOrEmpty(s.MediaUrl)) output.Add(s.MediaUri.AbsoluteUri);

                        for (int articleIndex = 0; articleIndex < s.ArticlesCount; articleIndex++)
                        {
                            Article a = s.GetArticle(articleIndex);
                            if (!string.IsNullOrEmpty(a.IconUrl)) output.Add(a.IconUri.AbsoluteUri);
                            if (!string.IsNullOrEmpty(a.MediaUrl)) output.Add(a.MediaUri.AbsoluteUri);
                        }
                    }
                }
            }
            return output;
        }

    }
}
