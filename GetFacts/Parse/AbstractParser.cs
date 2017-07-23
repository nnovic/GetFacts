using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
    public abstract class AbstractParser : IXPathNavigable
    {
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
            ClearSourceTree();
        }

        #region styling

        protected const double M_FONT_SIZE = 12;
        protected const double L_FONT_SIZE = 14;
        protected const double XL_FONT_SIZE = 16;

        protected readonly FontFamily sans_serif = new FontFamily("GlobalSanSerif.CompositeFont");


        public enum InformationType
        {
            ValuableClue,       // +++
            UsefulContent,      // ++
            MildlyInteresting,  // +
            NeutralData,        // =             
            MeaninglessJunk     // -
        }

        protected abstract InformationType EvaluateInformationType(object o);

        protected void Stylize(Inline te, object o)
        {
            InformationType it = EvaluateInformationType(o);
            Stylize(te, it);
        }

        private void Stylize(Inline te, InformationType it)
        {
            ApplyDefaultStyle(te);
            switch (it)
            {
                case InformationType.ValuableClue:
                    ApplyValuableClueStyle(te);
                    break;

                case InformationType.UsefulContent:
                    ApplyUsefulContentStyle(te);
                    break;

                case InformationType.MildlyInteresting:
                    ApplyMildlyInterestingStyle(te);
                    break;

                default:
                case InformationType.NeutralData:
                    break;

                case InformationType.MeaninglessJunk:
                    ApplyMeaninglessJunkStyle(te);
                    break;

            }
        }

        protected virtual void ApplyDefaultStyle(Inline te)
        {
            te.FontFamily = sans_serif;
            te.FontSize = M_FONT_SIZE;
            te.Foreground = Brushes.DimGray;
            te.TextDecorations = null;
        }

        protected virtual void ApplyValuableClueStyle(TextElement te)
        {
            te.Foreground = Brushes.Blue;
        }

        protected virtual void ApplyUsefulContentStyle(TextElement te)
        {
            te.Foreground = Brushes.Black;
            te.FontSize = XL_FONT_SIZE;
        }

        protected virtual void ApplyMildlyInterestingStyle(TextElement te)
        {
            te.Foreground = Brushes.Orange;
        }

        protected virtual void ApplyMeaninglessJunkStyle(TextElement te)
        {
            te.Foreground = Brushes.DarkGray;
        }

        #endregion

        #region [optionel] flow document avec le code source de la page

        protected virtual void ClearSourceCode()
        {
            foreach (Hyperlink hl in textElements2concreteObjects.TypedElements)
            {
                hl.Click -= Hyperlink_Click;
                hl.MouseEnter -= Hyperlink_MouseEnter;
                hl.MouseLeave -= Hyperlink_MouseLeave;
            }
            textElements2concreteObjects.Clear();

            if (sourceCode != null)
            {
                sourceCode.Blocks.Clear();
                sourceCode = null;
            }
        }

        private FlowDocument sourceCode = null;
        private DoubleList<TextElement> textElements2concreteObjects = new DoubleList<TextElement>();

        public FlowDocument SourceCode
        {
            get
            {
                if (sourceCode == null)
                {
                    sourceCode = CreateSourceCode();
                }
                return sourceCode;
            }
        }


        private FlowDocument CreateSourceCode()
        {
            FlowDocument flowDoc = new FlowDocument();            
            Paragraph mainSection = new Paragraph();            
            Span mainSpan = new Span();
            ApplyDefaultStyle(mainSpan);
            mainSection.Inlines.Add(mainSpan);
            flowDoc.Blocks.Add(mainSection);

            FillSourceCode(mainSpan);

            return flowDoc;
        }

        protected abstract void FillSourceCode(Span rootSpan);


        protected Hyperlink AddHyperlink(string text, object o)
        {
            Run r = new Run(text);
            Hyperlink hl = new Hyperlink(r);
            hl.Click += Hyperlink_Click;
            hl.MouseEnter += Hyperlink_MouseEnter;
            hl.MouseLeave += Hyperlink_MouseLeave;
            textElements2concreteObjects.Set(hl, o);
            Stylize(hl, o);
            return hl;
        }

        protected void AddTextElement(TextElement te, object o)
        {
            textElements2concreteObjects.Set(te, o);
        }

        private void Hyperlink_MouseLeave(object sender, MouseEventArgs e)
        {
            Hyperlink te = (Hyperlink)sender;
            te.TextDecorations = null;
        }

        private void Hyperlink_MouseEnter(object sender, MouseEventArgs e)
        {
            Hyperlink te = (Hyperlink)sender;
            te.TextDecorations = TextDecorations.Underline;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            object node = textElements2concreteObjects.GetObjectOf(hl);
            TreeViewItem tvi = treeViewItems2concreteObjects.GetTypedElementOf(node);
            tvi.IsSelected = true;
            tvi.BringIntoView();
            tvi.IsExpanded = true;
        }




        #endregion

        #region [optionel] tree view pour un élément précis du code source

        protected virtual void ClearSourceTree()
        {
            foreach(TreeViewItem treeNode in treeViewItems2concreteObjects.TypedElements)
            {
                // TODO: anything ?
            }
            treeViewItems2concreteObjects.Clear();
            sourceTreeRoot = null;
        }

        private TreeViewItem sourceTreeRoot = null;
        private DoubleList<TreeViewItem> treeViewItems2concreteObjects = new DoubleList<TreeViewItem>();

        public TreeViewItem SourceTree
        {
            get
            {
                if (sourceTreeRoot == null)
                {
                    sourceTreeRoot = CreateSourceTree();
                }
                return sourceTreeRoot;
            }
        }

        protected TreeViewItem AddTreeNode(object header, object o)
        {
            TreeViewItem treeNode = new TreeViewItem();
            treeNode.Header = header;
            treeViewItems2concreteObjects.Set(treeNode, o);
            return treeNode;
        }

        protected abstract TreeViewItem CreateSourceTree();

        #endregion


        #region xpath


        /// <summary>
        /// Suggère une requête XPath qui pourrait convenir pour obtenir 
        /// le noeud passé en paramètre.
        /// Attention: il ne s'agit pas d'obtenir un XPath qui retourne exactement
        /// ce seul noeud, mais bien de proposer une approximation, la plus
        /// élégante possible, qui pourra facilement être utilisée par l'utilisateur
        /// pour créer un Template.
        /// Dans la mesure du possible le XPath proposé tentera d'être le plus précis
        /// possible, mais ce n'est pas la priorité.
        /// </summary>
        /// <param name="tvi"></param>
        public string SuggestXPathFor(TreeViewItem tvi)
        {
            TreeViewItem selected = tvi;
            object concreteObject = treeViewItems2concreteObjects.GetObjectOf(selected);
            AbstractXPathBuilder builder = XPathFor(concreteObject);
            return builder.GetString();
        }

        protected abstract AbstractXPathBuilder XPathFor(object o);


        public IList<TextElement> SelectFromXPath(string xpath)
        {
            IList<object> concreteSelection = Select(xpath);
            List<TextElement> output = new List<TextElement>();

            foreach(object concreteObject in concreteSelection)
            {
                TextElement te = textElements2concreteObjects.GetTypedElementOf(concreteObject);
                output.Add(te);
            }

            return output;
        }

        protected abstract IList<object> Select(string xpath);

        #endregion


        public abstract XPathNavigator CreateNavigator();



        public static IEnumerable<string> AvailableParsers()
        {
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsSubclassOf(typeof(AbstractParser)) && t.IsAbstract == false
                        select t.Name;

            return types;
        }

        public static string DefaultParser
        {
            get { return typeof(HtmlParser).Name; }
        }

        /// <summary>
        /// Crée un instance concrète d'AbstractParser
        /// en se basant sur le nom de Type passé en argument.
        /// </summary>
        /// <param name="name">Nom du type à instancier. Doit être un type dérive d'AbstractParser.</param>
        /// <returns>L'object créé.</returns>
        public static AbstractParser NewInstance(string name)
        {
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsSubclassOf(typeof(AbstractParser)) && t.IsAbstract == false
                        select t;

            Type parserType = types.ElementAt(0);

            ConstructorInfo ci = parserType.GetConstructor(Type.EmptyTypes);
            object parser = ci.Invoke(null);
            return (AbstractParser)parser;
        }
    }
}
