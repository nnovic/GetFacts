using GetFacts.Download;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Facts
{
    public class Page:AbstractInfo
    {
        private readonly string pageUrl;
        private readonly Section defaultSection;
        private bool timerEnabled = false;

        public Page(string url)
        {
            pageUrl = url;
            BaseUri = new Uri(url, UriKind.Absolute);
            defaultSection = new Section(String.Empty);
            RefreshDelay = 10;
        }

        public Page(PageConfig pc):this(pc.Url)
        {
            Parser = new HtmlParser(); 
            Template = TemplateFactory.GetInstance().GetExistingTemplate(pc.Template);
            RefreshDelay = pc.Refresh * 60;
        }

        internal bool TimerEnabled
        {
            get { return timerEnabled; }
            set { timerEnabled = value; }
        }

        /// <summary>
        /// delai (en secondes) entre le moment
        /// où le téléchargement du document est terminé
        /// avec succès
        /// et le moment où il va recommencer.
        /// </summary>
        internal int RefreshDelay
        {
            get;
            set;
        }

        /// <summary>
        /// délai (en secondes) entre le moment
        /// où le téléchargement à échoué et celuis
        /// où l'on va le retenter.
        /// Utilisé aussi pour le temporiser le
        /// tout premier téléchargé de la page, au
        /// démarrage du programme.
        /// </summary>
        internal int RecoverDelay
        {
            get { return 10; }
        }

        #region configuration

        public AbstractParser Parser
        {
            get;set;
        }

        /// <summary>
        /// Retourne l'extension de fichier qui est
        /// la plus communément utilisée pour pour
        /// le type de fichier qui sert de source à
        /// cette page.
        /// Peut retourne null si cette information
        /// n'est pas disponible.
        /// </summary>
        /// <example>Pour des données au format HTML, DefaultFileExtension
        /// retournera ".html"</example>
        /// <remarks>Retourne le tout premier élément de Parser.UsualFileExtensions</remarks>
        public string DefaultFileExtension
        {
            get
            {
                return Parser.MostProbableFileExtension;
            }
        }

        public string Url
        {
            get
            {
                return pageUrl;
            }
        }

        public PageTemplate Template
        {
            get;set;
        }

        #endregion

        #region mise à jour 

        public void Update(string sourceFile)
        {            
            Parser.Load(sourceFile, Template.Encoding);
            Update(Parser.CreateNavigator());
        }

        private void Update(XPathNavigator nav)
        {            
            BeginUpdate();

            UpdateInfo(nav, Template);

            foreach (SectionTemplate sectionTemplate in Template.Sections)
            {
                XPathNavigator subTree = nav.SelectSingleNode(sectionTemplate.XPathFilter);
                string name = sectionTemplate.SectionName;
                Section s = GetSection(name);
                s.Update(subTree, sectionTemplate); 
            }

            EndUpdate();
        }

        #endregion

        #region Sections

        public int SectionsCount
        {
            get
            {
                int count = Children.Count;
                if (count == 0) return 1; // section par défaut, présente si aucune autre section n'est définie.
                else return count;
            }
        }

        public bool HasAnyArticle
        {
            get
            {
                for(int i=0;i<SectionsCount;i++)
                {
                    if (GetSection(i).ArticlesCount > 0)
                        return true;
                }
                return false;
            }

        }

        public Section GetSection(int index)
        {
            if (Children.Count == 0)
            {
                if (index == 0)
                    return defaultSection;
                else
                    throw new IndexOutOfRangeException();
            }
            else
            {
                return (Section)Children[index];
            }
        }

        public Section GetSection(string name)
        {
            if( string.IsNullOrEmpty(name) )
            {
                if(Children.Count==0)
                {
                    return defaultSection;
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }

            Section output = null;

            var sections = from child in Children
                           where child.Identifier == name
                           select child;


            if (sections.Count() == 0)
            {
                output = new Section(name)
                {
                    BaseUri = BaseUri
                };
                Children.Add(output);
            }
            else
            {
                output = sections.Single() as Section;
            }

            return output;
        }

        #endregion

        #region "comparaison"
        /*
        public static bool Compare(Page p1, Page p2)
        {
            return p1.CompareTo(p2);
        }

        public bool CompareTo(Page p)
        {
            if (string.Compare(pageUrl, p.pageUrl) != 0)
                return false;

            if( base.CompareTo(p)==false )
            {
                return false;
            }

            return true;
        }
        */
        #endregion


    }
}
