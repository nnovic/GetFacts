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

        /// <summary>
        /// Implémentation concrète de AbstractParser.EvaluateInformationType.
        /// Si o est de type HtmlNode, renvoie la valeur de EvaluateInformationype(HtmlNode).
        /// Si o est de type HtmlAttribute, renvoie la valeur de EvaluateInformationType(HtmlAttribute)
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <see cref="AbstractParser.EvaluateInformationType(object)"/>
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
            HtmlNode_To_Flow(mainNode, rootSpan);
        }
        
        /// <summary>
        /// Opération récursive qui parcourt l'arbre démarrant à 'node',
        /// et crée dans 'parent' un dérivé de TextElement approprié.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        /// <seealso cref=" HtmlNode_To_TreeView(HtmlNode, TreeViewItem)"/>
        private void HtmlNode_To_Flow(HtmlNode node, Span parent)
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
                HtmlNode_To_Flow(child, parent);
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

        
        #region tree

        private readonly double nodenameFontSize = XL_FONT_SIZE;

        protected override TreeViewItem CreateSourceTree()
        {
            HtmlNode rootHtmlNode = htmlDoc.DocumentNode;
            rootHtmlNode = rootHtmlNode.SelectSingleNode("/html");
            TreeViewItem root = HtmlElement_To_TreeViewItem(rootHtmlNode);

            foreach (HtmlNode child in rootHtmlNode.ChildNodes)
            {
                HtmlNode_To_TreeView(child, root);
            }

            root.ExpandSubtree();

            return root;
        }


        /*private void AppendAttributesToSpan(HtmlNode node, Span parent, bool providingCluesFiltering)
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
        }*/

        private TreeViewItem HtmlText_To_TreeViewItem(HtmlNode node)
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
        /// <param name="htmlNode"></param>
        /// <returns></returns>
        private TreeViewItem HtmlElement_To_TreeViewItem(HtmlNode htmlNode)
        {
            /*Span header = new Span()
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
            header.Inlines.Add(nodeName);*/

            Run header = new Run(htmlNode.Name);
            TreeViewItem nodeItem = AddTreeNode(header, htmlNode);


            // Attributs du noeud:
            foreach (HtmlAttribute attributeNode in htmlNode.Attributes)
            {
                Run attribute = new Run(
                    string.Format("@{0} = \"{1}\"",
                        attributeNode.Name,
                        attributeNode.Value));
                TreeViewItem attributeItem = AddTreeNode(attribute, attributeNode);
                nodeItem.Items.Add(attributeItem);
            }



            // mettre en priorité les attributs intéressants:
            //AppendAttributesToSpan(htmlNode, nodeItem, header, true);
            //AppendAttributesToSpan(node, header, false);          

            /*TreeViewItem tvi = new TreeViewItem();
            tvi.Header = header;
            return tvi;*/
            //return AddTreeNode(header, node);

            return nodeItem;
        }

        /// <summary>
        /// Opération récursive qui parcourt l'arbre démarrant à 'node',
        /// et crée dans 'parent' un TreeViewItem approprié.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        /// <seealso cref="HtmlNode_To_Flow(HtmlNode, Span)"/>
        void HtmlNode_To_TreeView(HtmlNode node, TreeViewItem parent)
        {            
            if (node.NodeType == HtmlNodeType.Element)
            {
                TreeViewItem tvi = HtmlElement_To_TreeViewItem(node);
                parent.Items.Add(tvi);

                // PROCESS CHILDREN
                foreach (HtmlNode child in node.ChildNodes)
                {
                    HtmlNode_To_TreeView(child, tvi);
                }
            }

            else if(node.NodeType==HtmlNodeType.Text)
            {
                TreeViewItem tvi = HtmlText_To_TreeViewItem(node);
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

        protected override AbstractXPathBuilder XPathFor(object o)
        {
            HtmlXPathBuilder builder = new HtmlXPathBuilder(htmlDoc.DocumentNode);
            builder.Build(o);
            builder.Optimize();
            return builder;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="navigators"></param>
        /// <param name="xpath"></param>
        /// <returns>Une liste non-nulle des XPathNavigator correspondants aux noeuds sélectionnées.
        /// Jamais null, mais peut être vide.</returns>
        private IList<XPathNavigator> SubSelect(IList<XPathNavigator> navigators, string xpath)
        {
            List<XPathNavigator> output = new List<XPathNavigator>();

            foreach(XPathNavigator navigator in navigators)
            {
                XPathNodeIterator selection = navigator.Select(xpath);
                foreach(XPathNavigator selected in selection)
                {
                    output.Add(selected);
                }
            }

            return output;
        }


        protected override System.Collections.Generic.IList<object> Select(params string[] xpathElements)
        {
            System.Collections.Generic.List<object> output = new System.Collections.Generic.List<object>();

            if ((xpathElements != null) && (xpathElements.Length > 1))
            {
                IList<XPathNavigator> navigators = new List<XPathNavigator>();
                navigators.Add(CreateNavigator());
             
                foreach(string xpath in xpathElements)
                {
                    if (!string.IsNullOrEmpty(xpath))
                    {
                        navigators = SubSelect(navigators, xpath);
                    }
                }

                foreach(XPathNavigator selected in navigators)
                {
                    if (selected is HtmlNodeNavigator nodeNav)
                    {
                        HtmlNode concreteNode = nodeNav.CurrentNode;
                        output.Add(concreteNode);
                    }
                    else
                    {

                    }
                }
            }

            return output;
        }



        #endregion
    }
}
