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
            IsNewBehavior = IsNewPropertyGets.OldImmediately;
        }

        protected AbstractInfo(string identifier) : this()
        {
            internalIdentifier = identifier;
        }

        #endregion

        #region Identifier

        string internalIdentifier = null;

        /// <summary>
        /// Une chaîne de caractère qui permet de différencier
        /// à coup sûr cette instance d'AbstractInfo de n'importe
        /// quelle autre. Si Identifier vaut null, il faudra utiliser
        /// une autre stratégie, comme par exemple se baser sur BrowserUrl
        /// ou la ressemblance des champs Title et Text.
        /// </summary>
        /// <remarks>Une fois initialisé à une valeur non nulle, il est interdit de modifier ce champ
        /// à nouveau. Sinon, une Exception sera lancée.</remarks>
        internal string Identifier
        {
            get
            {
                return internalIdentifier;
            }
            set
            {
                if (internalIdentifier != null)
                    throw new Exception("Identifier has already been set and cannot be changed");
                internalIdentifier = value;
            }
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

        #region Outdated/New

        public enum IsNewPropertyGets
        {
            /// <summary>
            /// IsNew retourne systématiquement false.
            /// </summary>
            NeverNew,

            /// <summary>
            /// IsNew passe immédiatement à false après avoir été lu. C'est le comportement par défaut.
            /// </summary>
            OldImmediately,

            /// <summary>
            /// IsNew ne devrait être forcé à false que lorsque le curseur de la souris
            /// a survolé le contrôle affichant les données de cet objet.
            /// </summary>
            OldOnMouseHover,

            /// <summary>
            /// IsNew ne devrait être forcé à false que lorsque l'utilisateur clique
            /// sur le contrôle qui affiche les données de cet objet.
            /// </summary>
            OldOnMouseClick,

            /// <summary>
            /// Is New est passé à false 
            /// </summary>
            OldOnRefresh
        }

        public static bool NewStatusForNewInstance = true;
        private bool newContent = NewStatusForNewInstance;
        private bool upToDate = false;

        /// <summary>
        /// Détermine sous quelle(s) condition(s) les données
        /// contenues dans cet objet passent de l'état "nouvelles" à
        /// "vieilles".
        /// </summary>
        /// <remarks>Par défaut, IsNewBehavior est initialisé à IsNewPropertyGets.OldImmediately dans
        /// le constructeur de la classe.</remarks>
        /// <see cref="IsNewPropertyGets"/>
        /// <seealso cref="IsNew"/>
        public IsNewPropertyGets IsNewBehavior
        {
            get;
            set;
        }


        /// <summary>
        /// Définit si les informations contenues dans cet objet
        /// sont nouvelles ou pas. Lorsqu'un objet de la classe AbstractInfo est 
        /// instancié, la valeur initiale de IsNew est la valeur de NewStatusForNewInstance
        /// (soit "true" par défaut).
        /// La valeur de IsNewBehavior est utilisée en association avec IsNew
        /// pour déterminer quand est-ce que les informations cessent d'être "nouvelles".
        /// </summary>
        /// <remarks>
        /// Le "setter" de cette propriété n'accepete que la valeur "false". Il est impossible
        /// de réaliser "IsNew=true;"
        /// </remarks>
        /// <seealso cref="IsNewBehavior"/>
        /// <see cref="NewStatusForNewInstance"/>
        public bool IsNew
        {
            get
            {
                if (newContent == true)
                {
                    switch (IsNewBehavior)
                    {
                        default:
                            return true;

                        case IsNewPropertyGets.OldImmediately:
                            newContent = false;
                            return true;

                        case IsNewPropertyGets.NeverNew:
                            newContent = false;
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true) throw new ArgumentException();
                newContent = false;
            }
        }

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

        internal Uri BaseUri
        {
            get; set;
        }

        Uri MakeUri(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            Uri output = new Uri(url, UriKind.RelativeOrAbsolute);
            if (output.IsAbsoluteUri == true)
                return output;

            output = new Uri(BaseUri, url);
            return output;
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
                return MakeUri(IconUrl);
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
                return MakeUri(MediaUrl);
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

        public Uri BrowserUri
        {
            get
            {
                return MakeUri(BrowserUrl);
            }
        }
        #endregion

        #region Content management

        protected void UpdateInfo(AbstractInfo source)
        {
            Title = source.Title;
            Text = source.Text;
            BaseUri = source.BaseUri;
            IconUrl = source.IconUrl;
            MediaUrl = source.MediaUrl;
            BrowserUrl = source.BrowserUrl;

            if (IsNewBehavior == IsNewPropertyGets.OldOnRefresh)
            {
                IsNew = false;
            }
        }

        /// <summary>
        /// Définit une limite du nombre caractères que peut
        /// avoir une chaine de caractères pour un affichage
        /// agréable à l'écran (les chaines trop longues donneront
        /// un affichage très seeré et illisible).
        /// </summary>
        private int MaxChars
        {
            get { return 280; }
        }

        protected void UpdateInfo(XPathNavigator nav, AbstractTemplate template)
        {
            if (!template.IdentifierTemplate.IsNullOrEmpty)
            {
                Identifier = template.IdentifierTemplate.Execute(nav);
            }

            Title = template.TitleTemplate.Execute(nav, MaxChars);
            Text = template.TextTemplate.Execute(nav, MaxChars);
            IconUrl = template.IconUrlTemplate.Execute(nav);
            MediaUrl = template.MediaUrlTemplate.Execute(nav);
            BrowserUrl = template.BrowserUrlTemplate.Execute(nav);

            if (IsNewBehavior == IsNewPropertyGets.OldOnRefresh)
            {
                IsNew = false;
            }
        }
        
        /// <summary>
        /// Returns true if this object has any non-empty string among
        /// the following attributes: Title, Text.
        /// Returns false if all theses attributes are null or empty strings, indicating
        /// that this object holds no valuable information.
        /// </summary>
        public virtual bool HasContent
        {
            get
            {
                if (string.IsNullOrEmpty(Title) == false) return true;
                if (string.IsNullOrEmpty(Text) == false) return true;
                return false;
            }
        }

        #endregion

    }
}
