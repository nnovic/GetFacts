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
        private readonly DownloadTask pageDownloadTask;
        private readonly Section defaultSection;
        private readonly Timer refreshTimer;
        private bool timerEnabled = false;

        public Page(string url)
        {
            pageUrl = url;
            BaseUri = new Uri(url, UriKind.Absolute);
            pageDownloadTask = DownloadManager.GetInstance().FindOrQueue(BaseUri);
            pageDownloadTask.TaskFinished += PageDownloadTask_TaskFinished;
            defaultSection = new Section(String.Empty);
            refreshTimer = new Timer(TimerProc);
        }

        public Page(PageConfig pc):this(pc.Url)
        {
            Parser = new HtmlParser(); 
            Template = TemplateFactory.GetInstance().GetTemplate(pc.Template); 
        }

        internal bool TimerEnabled
        {
            get { return timerEnabled; }
            set { timerEnabled = value; }
        }

        /// <summary>
        /// Should be called right after the Page has been instanciated.
        /// If the document is already available locally, it will trigger
        /// the parsing of the document, and the page will be available
        /// for display as soon as this method is finished.
        /// </summary>
        internal void Initialize()
        {
            pageDownloadTask.TriggerIfTaskFinished();
        }


        #region configuration

        protected AbstractParser Parser
        {
            get;set;
        }

        

        public string Url
        {
            get
            {
                return pageUrl;
            }
        }

        internal PageTemplate Template
        {
            get;set;
        }

        #endregion

        #region mise à jour 


        private void TimerProc(object state)
        {
            Console.WriteLine("The timer callback executes.");
            pageDownloadTask.Reload();
        }



        private void PageDownloadTask_TaskFinished(object sender, EventArgs e)
        {
            if(pageDownloadTask.Status== DownloadTask.DownloadStatus.Completed)
            {
                Parser.Load(pageDownloadTask.LocalFile);
                Update(Parser.CreateNavigator());
                if(TimerEnabled)
                    refreshTimer.Change(10 * 1000, Timeout.Infinite);
            }
            else
            {
                // SO WHAT ???
                if(TimerEnabled)
                    refreshTimer.Change(10 * 1000, Timeout.Infinite);
            }
        }

        void Update(XPathNavigator nav)
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

        public class SectionEventArgs:EventArgs
        {
            public Page Page { get; set; }
            public Section Section { get; set; }
        }

        public event EventHandler<SectionEventArgs> SectionAdded;

        protected void OnSectionAdded(Section s)
        {
            if(SectionAdded!=null)
            {
                SectionEventArgs args = new SectionEventArgs() { Page = this, Section = s };
                SectionAdded(this, args);
            }
        }

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
                OnSectionAdded(output);
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
