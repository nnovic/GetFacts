using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        /// Lit et charge en mémoire les données contenues dans le fichier spécifié
        /// par le paramètre "path". Ces données seront stockées dans une structure
        /// arborescente appropriée, comme par exemple un XmlDocument pour les fichiers
        /// XML. Cette structure arborescente sera ensuite parcourue à la recherche
        /// des informations pertinentes pour l'utilisateur.
        /// Le fichier source sera généralement un fichier texte, donc il faut gérer
        /// l'Encoding. Par défaut, l'auto-détection de l'encoding sera utilisée, mais
        /// on doit pouvoir forcer un autre encoding si le résultat de l'auto-détection
        /// n'est pas satisfaisant.
        /// </summary>
        /// <param name="path">Le chemin absolu du fichier contenant les données à analyser</param>
        /// <param name="encoding">Si null, l'encoding sera déterminé
        /// automatiquement</param>
        public abstract void Load(string path, Encoding encoding);

        /// <summary>
        /// Retourne une liste des extensions de fichier
        /// qui sont le plus couramment associées au
        /// type de resource qui est analysée par
        /// ce Parser. Cette liste doit être triée de l'extension 
        /// la plus courante à la moins courante.
        /// Les extensions doivent commencer par le '.' de séparation.
        /// </summary>
        /// <remarks>En pratique, c'est la toute première extension de cette liste qui sera utilisée.</remarks>
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

        /// <summary>
        /// Evalue le contenu de l'objet en paramètre et renvoie une information
        /// qui indique l'importance et/ou la pertinence de ce contenu. Permet
        /// notamment de choisir quel style sera utilisé pour affiche l'information
        /// à l'écran.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected abstract InformationType EvaluateInformationType(object o);

        /// <summary>
        /// Fonction qui applique un style au TextElement passé en paramètre
        /// en fonction du contenu de l'objet qui l'accompagne.
        /// La classe concrète doit implémenter EvaluateInformationType(object)
        /// en conséquence.
        /// </summary>
        /// <param name="te"></param>
        /// <param name="o"></param>
        /// <see cref="EvaluateInformationType(object)"/>
        /// <seealso cref="ApplyStyle(TextElement, InformationType)"/>
        protected void Stylize(TextElement te, object o)
        {
            InformationType it = EvaluateInformationType(o);
            ApplyStyle(te, it);
        }

        private void ApplyStyle(TextElement te, InformationType it)
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

        protected virtual void ApplyDefaultStyle(TextElement te)
        {
            te.FontFamily = sans_serif;
            te.FontSize = M_FONT_SIZE;
            te.Foreground = Brushes.DimGray;
            if (te is Inline il)
            {
                il.TextDecorations = null;
            }
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

        /// <summary>
        /// Cette méthode doit être implémentée par la classe fille.
        /// Son rôle est de remplir un FlowDocument dont le contenu reflête 
        /// le plus fidèlement possible les données structurées actuellement
        /// chargée, afin de les présenter sous forme de texte avec mise en forme
        /// et coloration syntaxique intelligente. 
        /// Cette méthode parcourera de façon systématique tous les noeuds de
        /// l'arborescence de données et produira des TextElement qui seront
        /// insérés dans le FlowDocument en utilisant "rootSpan" comme racine.
        /// </summary>
        /// <param name="rootSpan">racine pour insérer les TextElement qui
        /// constituent le flow document résultant de l'opération.</param>
        /// <seealso cref="AddHyperlink"/>
        /// <seealso cref="AddTextElement"/>
        protected abstract void FillSourceCode(Span rootSpan);

        protected void InsertText(Span parent, string text, object o)
        {
            text = text.Trim();

            if (string.IsNullOrEmpty(text) == false)
            {
                Hyperlink hlink = AddHyperlink(text, o);
                parent.Inlines.Add(hlink);
            }
        }

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

        #region [optionel] tree view avec le code source de la page

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

        /// <summary>
        /// Retourne une structure en arbre contenant
        /// une représentation du document qui a été
        /// chargé et parsé. Le travail de création de l'arbre
        /// est délégué à CreateSourceTree(), qui doit être implémenté
        /// dans la classe concrète.
        /// </summary>
        /// <see cref="CreateSourceTree"/>
        public TreeViewItem SourceTree
        {
            get
            {
                if (sourceTreeRoot == null)
                {
                    sourceTreeRoot = CreateSourceTree();
                    sourceTreeRoot.ExpandSubtree();
                }
                return sourceTreeRoot;
            }
        }

        /// <summary>
        /// A partir du document chargé, créer une arborsence de
        /// TreeViewItems qui représente la structure dudit document.
        /// </summary>
        /// <returns>Le TreeViewItem racine de l'arborescence créee.</returns>
        /// <remarks>Durant le process de création, la classe concrètre doit
        /// faire appel à AddTreeNode(object, object) pour que la 'magie'
        /// de la classe AbstractParser puisse opérer.</remarks>
        /// <see cref="AddTreeNode(object, object)"/>
        protected abstract TreeViewItem CreateSourceTree();

        protected TreeViewItem AddTreeNode(TextElement header, object o)
        {
            TreeViewItem treeNode = new TreeViewItem();
            treeNode.Header = header;
            treeViewItems2concreteObjects.Set(treeNode, o);
            Stylize(header, o);
            return treeNode;
        }

        protected TreeViewItem Text_To_TreeViewItem(string originalText, object o)
        {
            if (originalText == null)
                return null;

            string trimmedText = originalText.Trim();

            string compressedText = Regex.Replace(trimmedText, @"\s+", @" ");

            if (string.IsNullOrEmpty(compressedText))
                return null;

            Span header = new Span()
            {
                //FontFamily = textFontFamily,
                //Foreground = defaultColor
            };

            // Nom du noeud html: "#text"
            // Nom du noeud xpath : "text()"
            Run nodeName = new Run("text()")
            {
                //FontSize = nodenameFontSize,
                //Foreground = IsNodeMeaningless(node) ? defaultColor : textColor
            };
            header.Inlines.Add(nodeName);

            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run("| "));

            Run textRun = new Run(compressedText)
            {
                //Foreground = textColor,
                //FontSize = textFontSize,
                FontStyle = FontStyles.Italic
            };
            header.Inlines.Add(textRun);

            /*TreeViewItem tvi = new TreeViewItem();
            tvi.Header = header;
            return tvi;*/
            return AddTreeNode(header, o);
        }

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


        /// <summary>
        /// Retourne tous les objets de type TextElement qui correspondent
        /// à la requête XPath passée en argument. Cet requête peut être formée
        /// d'une seule string ou de de plusieurs, qui seront concaténées.
        /// </summary>
        /// <param name="xpathElements"></param>
        /// <returns></returns>
        /// <see cref="Select(string[])">Cette fait appel à Select(string[])</see>
        public IList<TextElement> SelectFromXPath(params string[] xpathElements)
        {
            IList<object> concreteSelection = Select(xpathElements);
            List<TextElement> output = new List<TextElement>();

            foreach(object concreteObject in concreteSelection)
            {
                TextElement te = textElements2concreteObjects.GetTypedElementOf(concreteObject);
                output.Add(te);
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xpathElements"></param>
        /// <returns>A non-null IList object. Might be empty, though.</returns>
        protected abstract IList<object> Select(params string[] xpathElements);

        #endregion


        public abstract XPathNavigator CreateNavigator();



        public static IEnumerable<string> AvailableParsers()
        {
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where t.IsSubclassOf(typeof(AbstractParser)) && t.IsAbstract == false
                        select t.Name;

            return types;
        }

        /// <summary>
        /// Default parser's name is "HtmlParser"
        /// </summary>
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

            var type = from t in types where t.Name.Equals(name) select t;
            Type parserType = type.First<Type>();

            ConstructorInfo ci = parserType.GetConstructor(Type.EmptyTypes);
            object parser = ci.Invoke(null);
            return (AbstractParser)parser;
        }
    }
}
