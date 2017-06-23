using GetFacts.Parse;
using GetFacts.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Facts
{
    public class Facts
    {
        private static Facts uniqueInstance = null;
        private readonly static object _lock_ = new object();

        public static Facts GetInstance()
        {
            lock(_lock_)
            {
                if(uniqueInstance==null)
                {
                    uniqueInstance = new Facts();
                }
                return uniqueInstance;
            }
        }

        //---------------------------------------------------------------------

        private Facts()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            List<PageConfig> config = ConfigFactory.GetInstance().CreateConfig("DefaultConfig.json");
            Page pageQuiFoire = new Page(config[0]);
            /*foreach(PageConfig pc in config)
            {
                Page p = new Page(pc);
                AddPage(p);
            }*/
            Page pageQuiMarche = new Demos.LbcPage("https://www.leboncoin.fr/annonces/offres/rhone_alpes/");

            PageTemplate.Compare(pageQuiFoire.Template, pageQuiMarche.Template);
            //AddPage(pageQuiMarche);
            /*Page p2 = new Demos.Dvp();
            AddPage(p2);*/
            AddPage(pageQuiFoire);
        }

        public void Initialize()
        {
            foreach(Page p in pages)
            {
                p.Initialize();
            }
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
        
        private Article More()
        {
            Section s = CurrentSection();
            if (s == null) return null;


            for (int tryArticle = 0; tryArticle <= s.ArticlesCount; tryArticle++)
            {
                Article a = More(s);
                if (a != null) return a;
            }

            return null;
        }

        private Article More(Section s)
        {
            if ((articleIndex + 1) >= s.ArticlesCount)
            {
                return null;
            }

            articleIndex++;
            Article a = s.GetArticle(articleIndex);

            if(outdatedItems.Contains(a) )
            {
                s.RemoveArticle(a);
                outdatedItems.Remove(a);
                a = null;
            }

            return a;
        }

        /// <summary>
        /// Trouve et retourne le prochain article à afficher, pris dans la liste
        /// actuelle de tous les articles disponibles en mémoire, et en fonction 
        /// de la position actuelle du curseur de lecture.
        /// La méthode peut également retourner "null" s'il n'y a pas d'article 
        /// à afficher. Cela peut se produire pour l'une des raisons suivantes:
        /// - Si "sectionChanged" vaut "true", il n'y avait plus d'article à lire 
        /// dans la section actuelle. La méthode a déjà déplacé le curseur de lecture
        /// dans la prochaine section, et il est possible de refaire immédiatement un
        /// appel à Next() pour obtenir le premier article de cette nouvelle section.
        /// - Si "pageChanged" vaut "true", il n'y avait plus d'article à lire dans
        /// la page actuelle (on avait donc déjà atteint le dernier article de la dernière
        /// section). La méthode a déjà déplacé le curseur de lecture dans la prochaine
        /// page, et il est possible de refaire immédiatement appel à Next() pour obtenir
        /// le premier article disponible dans la nouvelle page.
        /// - Sinon, il n'y a absolument aucun article à afficher. Il faut peut-être atteindre
        /// que les données aient été téléchargées par le gestionnaire de téléchargement...
        /// </summary>
        /// <param name="pageChanged"></param>
        /// <param name="sectionChanged"></param>
        /// <returns>Retourne le prochain article à afficher. Peut également retourner 'null'
        /// s'il n'y a pas d'autre article à afficher.
        /// </returns>
        private Article Next(out bool pageChanged, out bool sectionChanged)
        {
            pageChanged = false;
            sectionChanged = false;

            Page p = CurrentPage();
            Section s = CurrentSection();

            for (int tryPage=0; tryPage<=pages.Count; tryPage++)
            {
                if (p != null)
                {
                    for (int trySection = 0; trySection <= p.SectionsCount; trySection++)
                    {
                        if (s != null)
                        {
                            for (int tryArticle = 0; tryArticle <= s.ArticlesCount; tryArticle++)
                            {
                                /*articleIndex++;
                                if (articleIndex >= s.ArticlesCount)
                                {
                                    break;
                                }
                                Article a = s.GetArticle(articleIndex);*/
                                Article a = More(s);
                                if (a != null) return a;
                            }
                        }
                        articleIndex = -1;
                        sectionIndex++;
                        if (sectionIndex >= p.SectionsCount)
                        {
                            sectionIndex = 0;
                        }
                        s = p.GetSection(sectionIndex);
                        sectionChanged = true;
                    }
                }
                sectionIndex = -1;
                pageIndex++;
                if(pageIndex>=pages.Count)
                {
                    pageIndex = 0;
                }
                p = pages[pageIndex];
                pageChanged = true;
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
            if(a1!=null)
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


        private readonly List<Page> pages = new List<Page>();

        void AddPage(Page p)
        {
            lock (_lock_)
            {
                if (pages.Contains(p) == true)
                {
                    throw new ArgumentException();
                }

                //p.Outdated += Page_Outdated;
                p.SectionAdded += Page_SectionAdded;
                pages.Add(p);
            }
        }

        /*private void Page_Outdated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }*/

        #endregion

        #region Sections

        private void Page_SectionAdded(object sender, Page.SectionEventArgs e)
        {
            Section s = e.Section;
            s.Outdated += Section_Outdated;
            s.ArticleAdded += Section_ArticleAdded;
        }


        private void Section_Outdated(object sender, EventArgs e)
        {
            // Todo: mark for deletion all articles in the section.
            throw new NotImplementedException();
        }

        #endregion


        #region Articles
        

        private void Section_ArticleAdded(object sender, Section.ArticleEventArgs e)
        {
            Article a = e.Article;
            a.Outdated += Article_Outdated;
            Console.WriteLine("Facts: new article added");
        }



        #endregion

        #region outdated items

        private readonly List<AbstractInfo> outdatedItems = new List<AbstractInfo>();

        private void Article_Outdated(object sender, EventArgs e)
        {
            Article a = (Article)sender;
            a.Outdated -= Article_Outdated;
            Console.WriteLine("Facts: outdated article");

            lock (_lock_)
            {
                outdatedItems.Add(a);
            }
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
