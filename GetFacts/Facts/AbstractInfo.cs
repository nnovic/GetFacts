using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Facts
{
    public abstract class AbstractInfo
    {
        #region Constructors

        protected AbstractInfo()
        {
        }

        protected AbstractInfo(string identifier)
        {
            internalIdentifier = identifier;
        }

        #endregion

        #region Identifier

        string internalIdentifier;

        internal string Identifier
        {
            get { return internalIdentifier; }
        }

        #endregion

        #region Parent

        private AbstractInfo parent = null;

        bool HasParent
        {
            get { return parent != null; }
        }

        bool IsRoot
        {
            get { return !HasParent; }
        }

        #endregion

        #region Children

        private readonly List<AbstractInfo> children = new List<AbstractInfo>();

        bool HasChildren
        {
            get { return children.Count > 0; }
        }

        protected IList<AbstractInfo> Children
        {
            get { return children; }
        }

        #endregion

        #region Outdated

        private bool upToDate = false;

        /// <summary>
        /// Clears the "up-to-date" flag of this object
        /// and then does the same on all its children,
        /// recursively calling their BeginUpdate() method.
        /// </summary>
        protected void BeginUpdate()
        {
            upToDate = false;
            foreach (AbstractInfo child in children)
            {
                child.BeginUpdate();
            }
        }

        /// <summary>
        /// Check the state of the "up-to-date" flag of
        /// this object and its Children. If the "up-to-date"
        /// flag of this object is not raised (==false), returns false.
        /// If the "up-to-date" flag of one of its Children is not raised,
        /// remove it. Returns true if this object and its children are all
        /// up-to-date.
        /// </summary>
        protected bool EndUpdate()
        {
            if (upToDate == false)
            {
                return false;
            }

            List<AbstractInfo> toBeRemoved = new List<AbstractInfo>();
            foreach (AbstractInfo child in children)
            {
                if(child.EndUpdate()==false)
                {
                    toBeRemoved.Add(child);
                }
            }

            foreach(AbstractInfo deleted in toBeRemoved)
            {
                children.Remove(deleted);
            }

            return true;
        }

        protected void Updated()
        {
            upToDate = true;
        }

        #endregion

        #region Title

        private string titleReadFromTheWeb = null;

        public string Title
        {
            get
            {
                return titleReadFromTheWeb;
            }
            set
            {
                Updated();
                if (string.Compare(titleReadFromTheWeb, value) != 0)
                {
                    titleReadFromTheWeb = value;
                }
            }
        }

        #endregion

        #region Text

        private string textReadFromTheWeb = null;

        public string Text
        {
            get
            {
                return textReadFromTheWeb;
            }
            set
            {
                Updated();
                if (string.Compare(textReadFromTheWeb, value) != 0)
                {
                    textReadFromTheWeb = value;
                }
            }
        }

        #endregion

        #region BaseUri

       // private Uri baseUri = null;

        internal Uri BaseUri
        {
            get; set;
        }

        #endregion

        #region Icon

        private string iconUrl = null;

        public string IconUrl
        {
            get
            {
                return iconUrl;
            }
            set
            {
                Updated();
                if (string.Compare(iconUrl, value) != 0)
                {
                    iconUrl = value;
                }
            }
        }

        public Uri IconUri
        {
            get
            {
                if (BaseUri == null)
                    return new Uri(IconUrl, UriKind.Absolute);
                else
                    return new Uri(BaseUri, IconUrl);

            }
        }

        #endregion

        #region Media

        private string mediaUrl = null;

        public string MediaUrl
        {
            get
            {
                return mediaUrl;
            }
            set
            {
                Updated();
                if (string.Compare(mediaUrl, value) != 0)
                {
                    mediaUrl = value;
                }
            }
        }

        public Uri MediaUri
        {
            get
            {
                if (BaseUri == null)
                    return new Uri(MediaUrl, UriKind.Absolute);
                else
                    return new Uri(BaseUri, MediaUrl);

            }
        }


        #endregion

        #region BrowserUrl

        private string browserUrl = null;

        public string BrowserUrl
        {
            get
            {
                return browserUrl;
            }
            set
            {
                Updated();
                if (string.Compare(browserUrl, value) != 0)
                {
                    browserUrl = value;
                }
            }
        }

        #endregion

        protected void UpdateInfo(AbstractInfo source)
        {
            Title = source.Title;
            Text = source.Text;
            IconUrl = source.IconUrl;
            MediaUrl = source.MediaUrl;
            BrowserUrl = source.BrowserUrl;
        }

        protected void UpdateInfo(XPathNavigator nav, AbstractTemplate template)
        {
            if (template.TitleTemplate != null)
            {
                Title = template.TitleTemplate.Execute(nav);
            }
            else
            {
                Title = String.Empty;
            }

            if (template.TextTemplate != null)
            {
                Text = template.TextTemplate.Execute(nav);
            }
            else
            {
                Text = String.Empty;
            }

            if (template.IconUrlTemplate != null)
            {
                IconUrl = template.IconUrlTemplate.Execute(nav);
            }
            else
            {
                IconUrl = String.Empty;
            }

            if (template.MediaUrlTemplate != null)
            {
                MediaUrl = template.MediaUrlTemplate.Execute(nav);
            }
            else
            {
                MediaUrl = String.Empty;
            }

            if (template.BrowserUrlTemplate != null)
            {
                BrowserUrl = template.BrowserUrlTemplate.Execute(nav);
            }
            else
            {
                BrowserUrl = String.Empty;
            }
        }

        #region "comparaison"
        /*
        protected virtual bool CompareTo(AbstractInfo info)
        {
            if (string.Compare(Identifier, info.Identifier) != 0)
                return false;

            if (string.Compare(BaseUri.AbsoluteUri, info.BaseUri.AbsoluteUri) != 0)
                return false;

            if (string.Compare(Title, info.Title) != 0)
                return false;

            if (string.Compare(Text, info.Text) != 0)
                return false;

            if (string.Compare(IconUrl, info.IconUrl) != 0)
                return false;

            if (string.Compare(MediaUrl, info.MediaUrl) != 0)
                return false;

            if (string.Compare(BrowserUrl, info.BrowserUrl) != 0)
                return false;

            int childrenCount1= Children.Count;
            int childrenCount2 = info.Children.Count;
            if (childrenCount1 != childrenCount2)
                return false;

            for(int childIndex=0;childIndex<childrenCount1;childIndex++)
            {
                if (Children[childIndex].CompareTo(info.Children[childIndex]) == false)
                    return false;
            }

            return true;
        }
        */
        #endregion
    }
}
