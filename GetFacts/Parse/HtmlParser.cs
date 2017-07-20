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
        
        public override XPathNavigator CreateNavigator()
        {
            return htmlDoc.CreateNavigator();
        }

        protected override string XPathOf(object o)
        {
            if( o is HtmlNode node)
            {
                return XPathOf(node);
            }
            else if( o is HtmlAttribute)
            {
                HtmlAttribute attr = (HtmlAttribute)o;
                return attr.XPath;
            }
            return null;
        }

        private string XPathOf(HtmlNode node)
        {
            switch(node.NodeType)
            {
                case HtmlNodeType.Document:
                    return "/";

                case HtmlNodeType.Text:
                    return node.ParentNode.XPath + "/text()";

                case HtmlNodeType.Comment:
                    return node.ParentNode.XPath + "/comment()";

                default:
                case HtmlNodeType.Element:
                    return node.XPath;
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
    }
}
