using System;
using System.Collections.Generic;
using System.IO;
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
        /// Ouvre le fichier 'path' et appelle Load(Stream,Encoding).
        /// </summary>
        /// <param name="path">Le chemin absolu du fichier contenant les données à analyser</param>
        /// <param name="encoding">Si null, l'encoding sera déterminé
        /// automatiquement</param>
        /// <see cref="Load(Stream, Encoding)"/>
        public void Load(string path, Encoding encoding)
        {
            using (Stream streamReader = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Load(streamReader, encoding);
            }
        }

        /// <summary>
        /// Lit et charge en mémoire les données contenues provenant du stream passé
        /// en paramètre. Ces données seront stockées dans une structure
        /// arborescente appropriée, comme par exemple un XmlDocument pour les fichiers
        /// XML. Cette structure arborescente sera ensuite parcourue à la recherche
        /// des informations pertinentes pour l'utilisateur.
        /// La source sera généralement un texte, donc il faut gérer
        /// l'Encoding. Par défaut, l'auto-détection de l'encoding sera utilisée, mais
        /// on doit pouvoir forcer un autre encoding si le résultat de l'auto-détection
        /// n'est pas satisfaisant.
        /// </summary>
        /// <param name="stream">Source des données à analyser</param>
        /// <param name="encoding">Si null, l'encoding sera déterminé
        /// automatiquement</param>
        /// <remarks>
        /// La classe qui implémente cette méthode doit veiller à ce qu'elle
        /// puisse être appelée plusieurs fois avec des contenus différents: il
        /// faut donc prendre soin d'appeler "Clean()" avant de traiter le contenu
        /// de "stream".
        /// </remarks>
        public abstract void Load(Stream stream, Encoding encoding);        

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
            /// <summary>Niveau qui correspond aux noeuds et/ou
            /// aux attributs qui peuvent être utilisés afin de localiser une
            /// information de manière fiable dans la structure du document. C'est
            /// par exemple le cas des attributes "id" et "class" pour le Html.</summary>
            /// <remarks>
            /// +++
            /// </remarks>
            ValuableClue,

            /// <summary>Niveau qui correspond au contenu qui
            /// est susceptible d'être affiché par GetFactsApp: contenu
            /// ou titre d'article, etc. D'une manière générale, c'est le niveau
            /// pour les noeuds de type Text (càd les feuilles de l'arbre XmlDocument ou
            /// du HtmlDocument) ou bien les liens vers les images/vidéos accompagnant
            /// les textes.</summary>
            /// <remarks>
            /// ++
            /// </remarks>
            UsefulContent,

            /// <summary>Niveau intermédiaire (pour ne pas dire indéterminé!)
            /// qui correspond à un élément dont on hésite à dire s'il fournit
            /// des informations intéressantes ou pas. C'est par exemple le niveau
            /// pour les attributs XML dont la valeur retourne un résultat positif avec
            /// DownloadTypes.Guess(string)
            /// positif avec 
            /// </summary>
            /// <remarks>
            /// +
            /// </remarks>
            /// <seealso cref="DownloadTypes.Guess()"/>
            MildlyInteresting,

            /// <summary>Niveau neutre. Attribué par défaut à toute information
            /// qui ne rentre pas dans les autres catégories.</summary>
            /// <remarks>
            /// =
            /// </remarks>
            NeutralData,

            /// <summary>Niveau qui correspond à des portions du document
            /// qui n'ont aucune valeur pour GetFacts. Il s'agira par exemple
            /// des commentaires XML/HTML, du code Javascript, etc...</summary>
            /// <remarks>
            /// -
            /// </remarks>
            MeaninglessJunk
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

        /// <summary>
        /// Ajoute "text" dans le FlowDocument, dans un Run qui sera ajouté
        /// aux enfants de "parent". Ce texte n'est pas associé à un noeud 
        /// du document XML ou HTML. Le style par défaut est utilisé pour l'afficher.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// <see cref="ApplyDefaultStyle(TextElement)"/>
        /// <remarks>L'utilisation de cette méthode doit rester exceptionnelle. Autant
        /// que possible, utiliser la variante InsertText(Span,string,object).</remarks>
        protected void InsertText(Span parent, string text)
        {
            text = text.Trim();

            if (string.IsNullOrEmpty(text) == false)
            {
                Run r = new Run(text);
                ApplyDefaultStyle(r);
                parent.Inlines.Add(r);
            }
        }

        /// <summary>
        /// Ajoute "text" dans le FlowDocument, dans un Hyperlink qui sera ajouté
        /// aux enfants de "parent". Ce texte est pas associé à un noeud 
        /// du document XML ou HTML, ce qui permet de le relier à ce même noeud
        /// dans l'arborescence SourceTree. Voir AddHyperlink(string, object) pour
        /// plus d'infos.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// <param name="o"></param>
        /// <see cref="AddHyperlink(string, object)"/>
        /// <seealso cref="SourceTree"/>
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

        /// <summary>
        /// Cette méthode se déclenchera en cas de click sur un
        /// Hyperlink du SourceCode (Flow Document). Son but est
        /// de rendre actif le noeud équivalent dans SourceTree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <seealso cref="SourceCode"/>
        /// <seealso cref="SourceTree"/>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = TextElementToTreeViewItem((Hyperlink)sender);
            tvi.IsSelected = true;
            tvi.BringIntoView();
            tvi.IsExpanded = true;
        }

        /// <summary>
        /// Retourne un TreeViewItem appartenant à SourceTree et qui correspond
        /// au même noeud du document XML/HTML que le TextElement passé en argument.
        /// (Hyperlink appartenant à SourceCode, par exemple).
        /// </summary>
        /// <param name="te"></param>
        /// <returns></returns>
        /// <see cref="SourceTree"/>
        /// <see cref="SourceCode"/>
        public TreeViewItem TextElementToTreeViewItem(TextElement te)
        {
            object node = textElements2concreteObjects.GetObjectOf(te);
            TreeViewItem tvi = treeViewItems2concreteObjects.GetTypedElementOf(node);
            return tvi;
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
        /// <param name="root">Si non vide, l'expression XPath sera relative par ra</param>
        public string SuggestXPathFor(TreeViewItem tvi, string root)
        {
            TreeViewItem selected = tvi;
            object concreteObject = treeViewItems2concreteObjects.GetObjectOf(selected);
            AbstractXPathBuilder builder = XPathFor(concreteObject, root);
            return builder.GetString();
        }

        /// <summary>
        /// Créé une expression XPath qui est adaptée pour trouver
        /// l'objet passé en paramètre.
        /// L'expression retournée est la plus simple possible.
        /// Cette méthode n'est pas "bijective", c'est-à-dire que
        /// l'XPath retournée pourra donner plusieurs correpondances,
        /// dont "o" fera partie.
        /// </summary>
        /// <param name="target">objet concret que l'expression XPath devra être capable de trouver.</param>
        /// <param name="startingPoint"">si renseignée, cette expression XPath devra être le point de départ de la construction de l'expression XPath retournée. Sinon, on part de la racine du document</param>
        /// <returns>Une instance de AbstractXPathBuilder</returns>
        private AbstractXPathBuilder XPathFor(object target, string startingPoint)
        {
            AbstractXPathBuilder builder = CreateXPathBuilder();
            builder.Build(target);

            if(string.IsNullOrEmpty(startingPoint)==false)
            {
                // obtenir une liste d'objets concrets à partir
                // de l'expression "startingPoint":
                IList<object> roots = Select(startingPoint);
                foreach(object root in roots )
                {
                    // construire un AbstractXPathBuilder,
                    AbstractXPathBuilder tmpBuilder = CreateXPathBuilder();

                    // faire un Build() dessus,
                    tmpBuilder.Build(root);

                    builder.MakeRelative(tmpBuilder);
                }
            }

            builder.Optimize();
            return builder;
        }

        protected abstract AbstractXPathBuilder CreateXPathBuilder();

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
        /// Retourne la liste des objets concrets qui "matchent" avec 
        /// l'expression XPath passée en paramètre.
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
