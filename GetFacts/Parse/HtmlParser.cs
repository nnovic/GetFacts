using HtmlAgilityPack;
using System.Collections;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GetFacts.Parse
{
    public class HtmlParser:AbstractParser
    {
        private HtmlDocument htmlDoc = null;

        public HtmlParser()
        {
            htmlDoc = new HtmlDocument();
        }

        public override void Load(string path, Encoding encoding)
        {
            Clear();
            if (encoding != null)
            {
                htmlDoc.Load(path, encoding);
            }
            else
            {
                htmlDoc.Load(path, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Should be ordering from most common to less common file extension.</remarks>
        public override string[] UsualFileExtensions
        {
            get { return new string[] { ".html", ".htm" }; }
        }


        public override void Clear()
        {
            base.Clear();
            /*if( htmlDoc!=null)
            {
                htmlDoc = null;
            }*/
        }



        #region styling

        protected override InformationType EvaluateInformationType(object o)
        {
            if( o is HtmlNode)
            {
                return EvaluateInformationType((HtmlNode)o);
            }
            else if(o is HtmlAttribute)
            {
                return EvaluateInformationType((HtmlAttribute)o);
            }
            return InformationType.NeutralData;
        }

        private InformationType ScriptInformationType => InformationType.MeaninglessJunk;


        private InformationType EvaluateInformationType(HtmlNode node)
        {
            string nodeName = node.Name.ToLower().Trim();
            string parentName = node.ParentNode.Name.ToLower().Trim();

            switch (nodeName)
            {
                case "script":
                    return ScriptInformationType;
            }

            switch (parentName)
            {
                case "script":
                    return ScriptInformationType;
            }

            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    return InformationType.MeaninglessJunk;

                case HtmlNodeType.Text:
                    return InformationType.UsefulContent;
            }

            return InformationType.NeutralData;
        }


        private InformationType EvaluateInformationType(HtmlAttribute attr)
        {
            HtmlNode node = null;
            string nodeName = null;
            string attributeName = null;


            attributeName = attr.Name.ToLower().Trim();
            node = attr.OwnerNode;
            nodeName = node.Name.ToLower().Trim();

            switch (nodeName)
            {
                case "script":
                    return ScriptInformationType;

                case "img":
                    {
                        switch (attributeName)
                        {
                            case "src:":
                                return InformationType.UsefulContent;
                        }
                    }
                    break;
            }

            switch (attributeName)
            {
                case "id":
                case "class":
                    return InformationType.ValuableClue;
            }

            return InformationType.NeutralData;
        }


        #endregion


        #region flow document

        protected override void FillSourceCode(Span rootSpan)
        {
            HtmlNode mainNode = htmlDoc.DocumentNode;
            Html_To_Flow(mainNode, rootSpan);
        }

        
        private void Html_To_Flow(HtmlNode node, Span parent)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                case HtmlNodeType.Text:
                    HtmlText_To_Run(node, parent);
                    return;
            }

            if (node.NodeType != HtmlNodeType.Document)
            {
                Span globalSpan = new Span();
                parent.Inlines.Add(globalSpan);
                Stylize(globalSpan, node);

                // HTML TAG (opening)
                Run openingTag = new Run();
                openingTag.Text = string.Format("<{0}", node.OriginalName);
                globalSpan.Inlines.Add(openingTag);

                // ATTRIBUTES ?
                if (node.HasAttributes)
                {
                    foreach (HtmlAttribute attr in node.Attributes)
                    {
                        HtmlAttribute_To_Runs(attr, globalSpan);
                    }
                }

                // HTML TAG (close opening tag)
                if (node.HasChildNodes == false)
                {
                    globalSpan.Inlines.Add(" />");
                }
                else
                {
                    globalSpan.Inlines.Add(">");
                }

                parent = globalSpan;
                AddTextElement(globalSpan, node);
            }

            // PROCESS CHILDREN
            foreach (HtmlNode child in node.ChildNodes)
            {
                Html_To_Flow(child, parent);
            }


            // HTML TAG (closing)
            if ((node.NodeType != HtmlNodeType.Document) && (node.HasChildNodes == true))
            {
                Run closingTag = new Run(string.Format("</{0}>", node.OriginalName));
                parent.Inlines.Add(closingTag);
            }
        }

        

        void HtmlAttribute_To_Runs(HtmlAttribute attr, Span parent)
        {
            Run r1 = new Run(string.Format(" {0}=\"", attr.OriginalName));
            parent.Inlines.Add(r1);

            if (string.IsNullOrEmpty(attr.Value) == false)
            {
                Hyperlink r2 = AddHyperlink(attr.Value, attr);
                //r2.Foreground = defaultColor;
                parent.Inlines.Add(r2);

                /*if (IsAttributeProvidingData(attr))
                {
                    r2.Foreground = attributeColor;
                    r2.FontSize = attributeFontSize;
                }*/
            }

            Run r3 = new Run("\"");
            parent.Inlines.Add(r3);
        }

        void HtmlText_To_Run(HtmlNode node, Span parent)
        {
            string text = node.InnerText;

            text = text.Trim();

            if (string.IsNullOrEmpty(text) == false)
            {
                Hyperlink hlink = AddHyperlink(text, node);
                //hlink.Foreground = defaultColor;

                /*if (string.Compare(node.ParentNode.Name, "script", true) == 0)
                {
                    hlink.FontFamily = scriptFontFamily;
                    hlink.Foreground = scriptColor;
                }
                else if (string.Compare(node.ParentNode.Name, "style", true) == 0)
                {
                    hlink.Foreground = styleColor;
                }
                else
                {
                    if (node.NodeType == HtmlNodeType.Text)
                    {
                        hlink.Foreground = textColor;
                        hlink.FontFamily = textFontFamily;
                        hlink.FontSize = textFontSize;
                    }
                    else if (node.NodeType == HtmlNodeType.Comment)
                    {
                        hlink.Foreground = commentColor;
                    }
                }*/
                parent.Inlines.Add(hlink);
            }
        }

        #endregion


        /*
        protected override Hashtable GetConcreteAttributesOf(object o)
        {
            Hashtable output = new Hashtable();

            if (o is HtmlAttribute)
            {
                HtmlAttribute attr = (HtmlAttribute)o;
            }
            else if (o is HtmlNode)
            {
                HtmlNode node = (HtmlNode)o;
                foreach(HtmlAttribute attr in node.Attributes)
                {
                    output.Add(attr.OriginalName, attr.Value);
                }
            }
            else
            {
                throw new ArgumentException();
            }

            return output;
        }
        */


        #region tree

        private readonly double nodenameFontSize = XL_FONT_SIZE;



        protected override TreeViewItem CreateSourceTree()
        {
            HtmlNode rootHtmlNode = htmlDoc.DocumentNode;
            rootHtmlNode = rootHtmlNode.SelectSingleNode("/html");
            TreeViewItem root = CreateElementTvi(rootHtmlNode);

            foreach (HtmlNode child in rootHtmlNode.ChildNodes)
            {
                Html_To_TreeViewItem(child, root);
            }

            root.ExpandSubtree();

            return root;
        }


        private void AppendAttributesToSpan(HtmlNode node, Span parent, bool providingCluesFiltering)
        {
            foreach (HtmlAttribute attr in node.Attributes)
            {
                //if (IsAttributeProvidingClues(attr) == providingCluesFiltering)
                {
                    parent.Inlines.Add(new LineBreak());
                    parent.Inlines.Add(new Run("| ") { });

                    Span span = new Span();
                    span.Inlines.Add(new Run(string.Format("@{0}", attr.Name)) { });
                    span.Inlines.Add(new Run(" = ") { });
                    span.Inlines.Add(new Run("\"") { });
                    span.Inlines.Add(new Run(attr.Value) { });
                    span.Inlines.Add(new Run("\"") { });
                    parent.Inlines.Add(span);

                    if( providingCluesFiltering==true )
                    {
                        span.Foreground = Brushes.Orange;
                    }
                }
            }
        }

        private TreeViewItem CreateTextTvi(HtmlNode node)
        {
            string originalText = node.InnerText;
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
            return AddTreeNode(header, node);
        }

        /// <summary>
        /// Crée un TreeViewItem qui permet de rendre le contenu
        /// du HtmlNode passé en argument.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private TreeViewItem CreateElementTvi(HtmlNode node)
        {
            Span header = new Span()
            {
                //FontFamily = defaultFontFamily,
                //FontSize = defaultFontSize,
                //Foreground = defaultColor
            };
      
            // Nom du noeud. Ex: "DIV"
            Run nodeName = new Run(node.Name)
            {
                FontSize = nodenameFontSize,
                //Foreground = IsNodeMeaningless(node) ? defaultColor : Brushes.Blue
            };
            header.Inlines.Add(nodeName);

            // Attributs du noeud:
            // mettre en priorité les attributs intéressants:
            AppendAttributesToSpan(node, header, true);
            //AppendAttributesToSpan(node, header, false);          

            /*TreeViewItem tvi = new TreeViewItem();
            tvi.Header = header;
            return tvi;*/
            return AddTreeNode(header, node);
        }

        void Html_To_TreeViewItem(HtmlNode node, TreeViewItem parent)
        {            
            if (node.NodeType == HtmlNodeType.Element)
            {
                TreeViewItem tvi = CreateElementTvi(node);
                parent.Items.Add(tvi);

                // PROCESS CHILDREN
                foreach (HtmlNode child in node.ChildNodes)
                {
                    Html_To_TreeViewItem(child, tvi);
                }
            }

            else if(node.NodeType==HtmlNodeType.Text)
            {
                TreeViewItem tvi = CreateTextTvi(node);
                if (tvi != null)
                {
                    parent.Items.Add(tvi);
                }
            }

        }

        #endregion


        #region xpath

        public override XPathNavigator CreateNavigator()
        {
            return htmlDoc.CreateNavigator();
        }

        /// <summary>
        /// Suggère une requête XPath qui pourrait convenir pour obtenir 
        /// l'objet passé en paramètre.
        /// Attention: il ne s'agit pas d'obtenir un XPath qui retourne exactement
        /// l'objet et lui seul, mais bien de proposer une approximation, la plus
        /// élégante possible, qui pourra facilement être utilisée par l'utilisateur
        /// pour créer un Template.
        /// Dans la mesure du possible le XPath proposé tentera d'être le plus précis
        /// possible, mais ce n'est pas la priorité.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected override string XPathFor(object o)
        {
            if( o is HtmlNode node)
            {
                return XPathOf(node);
            }
            else if( o is HtmlAttribute attr)
            {
                HtmlNode parent = attr.OwnerNode;
                return string.Format("{0}/@{1}", XPathOf(parent), attr.Name);
            }
            throw new ArgumentException();
        }

        private string XPathOf(HtmlNode node)
        {            
            switch (node.NodeType)
            {
                case HtmlNodeType.Document:
                    return "/";

                case HtmlNodeType.Text:
                    // TODO: est-ce vraiment nécessaire, en fait ?
                    return CreateXPathFor(node.ParentNode) + "/text()";

                case HtmlNodeType.Comment:
                    return CreateXPathFor(node.ParentNode) + "/comment()";

                default:
                case HtmlNodeType.Element:
                    return CreateXPathFor(node);
            }
        }

        /// <summary>
        /// Rule #1: look for HTML nodes with an "id" attribute, from leaf to root, and
        /// start building the XPath from the first matching node. The resulting XPath
        /// will start with the //*[@id="the value of the id attribute"] pattern.
        /// 
        /// Rule #2: for each node, if there is no other sibling node with the
        /// same tag name, do not decorate that node's name in the resulting XPath.
        /// Example: by default, HtmlAgilityPack's XPath navigator would return "/html[1]/head[1]/title[1]"
        /// instead of "/html/head/title ". This method, however, will return "/html/head/title" as expected.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string CreateXPathFor(HtmlNode node)
        {
            List<HtmlNode> hierarchy = new List<HtmlNode>(node.AncestorsAndSelf());

            // find first node with "id" attribute.
            // "id" is supposed to be unique in the entire html page
            int indexOfNodeWithId = -1;
            for(int index=0; index<hierarchy.Count; index++)
            {
                HtmlNode n = hierarchy[index];
                string id = n.GetAttributeValue("id", null);
                if( string.IsNullOrEmpty(id) == false )
                {
                    indexOfNodeWithId = index;
                    break;
                }

            }

            if( indexOfNodeWithId!= -1)
            {
                int index = indexOfNodeWithId + 1;
                int count = hierarchy.Count - (index);
                hierarchy.RemoveRange(index, count);
            }

            hierarchy.Reverse();



            StringBuilder sb = new StringBuilder();

            foreach(HtmlNode n in hierarchy)
            {
                switch(n.NodeType)
                {
                    case HtmlNodeType.Document:
                        break;

                    case HtmlNodeType.Element:
                        sb.Append('/').Append(SuggestXPathFor(n));
                        break;

                    // les cas ci-dessous ne devraient
                    // jamais se produire.
                    default:
                    case HtmlNodeType.Text:
                    case HtmlNodeType.Comment:
                        throw new Exception();
                }
            }

            string result = sb.ToString();
            return result;
        }


        /// <summary>
        /// Dans l'objectif de construire un XPath complet pour un noeud HTML,
        /// cette méthode a pour but de suggérer le XPath le plus approprié pour
        /// qualifer le noeud passé en paramètre. Le reste de la hiérarchie est ignoré.
        /// En faisait appel à cette méthode pour chaque noeud HTML de la hiérarchie, on
        /// pourra construire un XPath complet.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string SuggestXPathFor(HtmlNode node)
        {
            StringBuilder sb = new StringBuilder();

            string _id = node.GetAttributeValue("id", string.Empty);
            if (string.IsNullOrEmpty(_id) == false)
            {
                sb.AppendFormat("/*[@id=\"{0}\"]", _id);
            }
            else
            {
                sb.Append(node.Name);

                // Chercher si d'autre noeuds HTML du même niveau portent le même nom;
                // Si c'est le cas, il va falloir trouver un moyen de différencier 
                // le noeud en paramètres des autres noeuds similaires.
                List<HtmlNode> siblings = new List<HtmlNode>(node.ParentNode.Elements(node.Name));
                if (siblings.Count > 1)
                {
                    List<string> interestingAttributes = new List<string>();

                    // Rechercher en priorité les attributs "préférés" pour le noeud en paramètre
                    // et, s'il y a correspondance, mettre ces attributs dans la liste des
                    // attributs qui figureront dans le xpath.
                    foreach(string attributeName in MostSignificantAttributesFor(node))
                    {
                        foreach(HtmlAttribute attributeNode in node.ChildAttributes(attributeName))
                        {
                            string interestingAttribute = ToXPathString(attributeNode);

                            // cependant, si tous les autres noeuds similaires
                            // possèdent exactement le même attribut avec la même valeur,
                            // ça n'a plus d'intérêt:
                            if( AllHtmlNodesHaveTheSameAttributeValue(siblings, attributeNode) ==false )
                                interestingAttributes.Add(interestingAttribute);
                        }
                    }


                    // Rechercher ensuite quel(s) attribut(s) possède le noeud en paramètres que
                    // les autres noeuds similaires ne possèderaient pas.
                    List<string> uniqueAttributes = new List<string>(from attr in node.Attributes select ToXPathString(attr));
                    siblings.ForEach(s => { if(s!=node) s.Attributes.ToList().ForEach(a=> { uniqueAttributes.Remove(ToXPathString(a)); }); });
                    uniqueAttributes.ForEach(s =>
                    {
                        if (!interestingAttributes.Contains(s)) interestingAttributes.Add(s);
                    });


                    if( interestingAttributes.Count >0 )
                    {
                        sb.Append('[');
                        for(int i=0;i<interestingAttributes.Count;i++)
                        {
                            if (i > 0)
                                sb.Append(" and ");
                            sb.Append(interestingAttributes[i]);
                        }
                        sb.Append(']');
                    }
                    /*else
                    {
                        int index = siblings.IndexOf(node);
                        sb.AppendFormat("[{0}]", index+1);
                    }*/
                }

                
            }
            return sb.ToString();
        }

        private bool AllHtmlNodesHaveTheSameAttributeValue(ICollection<HtmlNode> nodes, HtmlAttribute attribute)
        {
            foreach(HtmlNode node in nodes)
            {
                List<HtmlAttribute> attributesWithSameName = node.ChildAttributes(attribute.Name).ToList();
                if (attributesWithSameName.Count == 0)
                    return false;

                var attributesWithSameValue = from attr in attributesWithSameName where attr.Value == attribute.Value select attr;
                if (attributesWithSameValue.Count() == 0)
                    return false;
            }
            return true;
        }

        private string ToXPathString(HtmlAttribute attr)
        {
            return string.Format("@{0}=\"{1}\"", attr.Name, attr.Value);
        }

        private string[] MostSignificantAttributesFor(HtmlNode node)
        {
            switch(node.Name.ToLower())
            {
                case "meta":
                    return new string[] { "name" };

                default:
                    return new string[] {"class", "title"};
            }
        }

        protected override System.Collections.Generic.IList<object> Select(string xpath)
        {
            XPathNavigator nav = CreateNavigator();
            XPathNodeIterator result = nav.Select(xpath);
            System.Collections.Generic.List<object> output = new System.Collections.Generic.List<object>();

            foreach(var e in result)
            {
                if( e is HtmlNodeNavigator nodeNav)
                {
                    HtmlNode concreteNode = nodeNav.CurrentNode;
                    output.Add(concreteNode);
                }
                else
                {

                }
            }

            return output;
        }

        #endregion
    }
}
