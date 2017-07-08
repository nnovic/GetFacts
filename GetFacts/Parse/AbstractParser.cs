using GetFacts.Facts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    /// <summary>
    /// La classe AbstractParser est le prototype
    /// pour les classes en charges de tansformer
    /// les documents téléchargés en informations
    /// affichables dans GetFacts.
    /// La méthode Load() doit être utilisée en premier pour
    /// charger le contenu du document téléchargé. Ensuite,
    /// il est possible d'utiliser CreateNavigator() pour obtenir
    /// un objet permettant de naviguer dans la structure dudit document.
    /// </summary>
    public abstract class AbstractParser:IXPathNavigable
    {

        protected abstract Hashtable GetConcreteAttributesOf(object o);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding">Si null, l'encoding sera déterminé
        /// automatiquement</param>
        public abstract void Load(string path, Encoding encoding);

        /// <summary>
        /// Retourne une liste des extensions de fichier
        /// qui sont le plus couramment associées au
        /// type de resource qui est analysée par
        /// ce Parser.
        /// </summary>
        /// <remarks>Should be ordering from most common to less common file extension.</remarks>
        /// <example>HtmlParser retournera {".html", ".htm"}</example>
        public abstract string[] UsualFileExtensions
        {
            get;
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
        /// <remarks>Retourne le tout premier élément de UsualFileExtensions</remarks>
        public string MostProbableFileExtension
        {
            get
            {
                if ((UsualFileExtensions == null)
                    || (UsualFileExtensions.Length < 1))
                    return null;
                else
                    return UsualFileExtensions[0];
            }
        }
        

        /// <summary>
        /// TO BE IMPROVED !!
        /// </summary>
        public virtual void Clear()
        {
            ClearSourceCode();
            ClearSelectedSource();
        }       

        #region [optionel] flow document avec le code source de la page

        protected virtual void ClearSourceCode()
        {
            foreach (Hyperlink hl in hyperlinksToConcreteType.Keys)
            {
                hl.Click -= Output_Click;
                hl.MouseEnter -= Hl_MouseEnter;
                hl.MouseLeave -= Hl_MouseLeave;
            }
            hyperlinksToConcreteType.Clear();

            if (sourceCode != null)
            {
                sourceCode.Blocks.Clear();
                sourceCode = null;
            }
        }

        private FlowDocument sourceCode = null;
        private Hashtable hyperlinksToConcreteType = new Hashtable();

        public FlowDocument SourceCode
        {
            get
            {
                if( sourceCode==null)
                {
                    sourceCode = CreateSourceCode();
                }
                return sourceCode;
            }
        }

        protected abstract FlowDocument CreateSourceCode();

       /* bool HasSourceCode
        {
            get { return sourceCode != null; }
        }*/

        protected abstract string GetConcreteXPathOf(object o);

        protected abstract TextElement GetTextElementAssociatedWith(object o);

        protected Hyperlink AddHyperlink(string text, object o)
        {
            Run r = new Run(text);
            Hyperlink hl = new Hyperlink(r);
            hl.Click += Output_Click;
            hl.TextDecorations = null;
            hl.Foreground = null;
            hl.MouseEnter += Hl_MouseEnter;
            hl.MouseLeave += Hl_MouseLeave;
            hyperlinksToConcreteType.Add(hl, o);
            return hl;
        }

        private void Hl_MouseLeave(object sender, MouseEventArgs e)
        {
            Hyperlink te = (Hyperlink)sender;
            te.TextDecorations = null;
        }

        private void Hl_MouseEnter(object sender, MouseEventArgs e)
        {
            Hyperlink te = (Hyperlink)sender;
            te.TextDecorations = TextDecorations.Underline;
        }

        private void Output_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            object node = hyperlinksToConcreteType[hl];

            TreeViewItem leaf = null;
            try
            {
                //SelectedSource.BeginInit();
                SelectedSource.Items.Clear();
                UpdateCodeTree(node, out leaf);
                SelectedSource.ExpandSubtree();
                if (leaf != null) leaf.IsSelected = true;
            }
            finally
            {
                //SelectedSource.EndInit();
            }
        }

        #endregion

        #region [optionel] tree view pour un élément précis du code source

        protected virtual void ClearSelectedSource()
        {
            if( selectedCodePath!=null)
            {
                selectedCodePath.Items.Clear();
                selectedCodePath = null;
            }
        }

        private TreeViewItem selectedCodePath = null;
        
        public TreeViewItem SelectedSource
        {
            get
            {
                if( selectedCodePath==null )
                {
                    selectedCodePath = new TreeViewItem()
                    {
                        Header = "Root"
                    };
                }
                return selectedCodePath;
            }
        }
        
        protected abstract void UpdateCodeTree(object o, out TreeViewItem leaf);

        public Hashtable GetAttributesOf(TreeViewItem tvi)
        {
            if (tvi == null)
                return null;

            object node = tvi.Tag;
            if (node == null)
                return null;

            return GetConcreteAttributesOf(node);
        }

        public TextElement GetTextElementOf(TreeViewItem tvi)
        {
            if (tvi == null)
                return null;

            object node = tvi.Tag;
            if (node == null)
                return null;

            return GetTextElementAssociatedWith(node);
        }

        public string GetXPathOf(TreeViewItem tvi)
        {
            if (tvi == null)
                return string.Empty;

            object node = tvi.Tag;
            if (node == null)
                return string.Empty;

            return GetConcreteXPathOf(node);
        }

        #endregion

        /*
        #region extraction des infos du code source

        private XmlDocument xmlDoc = null;

        public XmlDocument Document
        {
            get
            {
                if( xmlDoc==null )
                {
                    xmlDoc = new XmlDocument();
                }
                return xmlDoc;
            }
        }

        #endregion
        */

        public abstract XPathNavigator CreateNavigator();
    }
}
