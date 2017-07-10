using HtmlAgilityPack;
using System.Collections;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml.XPath;

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

        #region flow document

        private readonly FontFamily defaultFontFamily = new FontFamily("GlobalSanSerif.CompositeFont");
        private readonly double defaultFontSize = 12;
        private readonly Brush defaultColor = Brushes.DarkGray;

        private readonly FontFamily scriptFontFamily = new FontFamily("GlobalMonospace.CompositeFont");
        private readonly Brush scriptColor = Brushes.Blue;

        private readonly FontFamily textFontFamily = new FontFamily("GlobalSerif.CompositeFont");
        private readonly double textFontSize = 16;
        private readonly Brush textColor = Brushes.Black;

        private readonly double attributeFontSize = 14;
        private readonly Brush attributeColor = Brushes.DimGray;

        private readonly Brush styleColor = Brushes.Purple;

        private readonly Brush commentColor = Brushes.Green;

        protected override FlowDocument CreateSourceCode()
        {
            FlowDocument flowDoc = new FlowDocument()
            {
                FontFamily = defaultFontFamily,
                FontSize = defaultFontSize,
                Foreground = defaultColor
            };
            Paragraph mainSection = new Paragraph();
            Span mainSpan = new Span();
            mainSection.Inlines.Add(mainSpan);
            flowDoc.Blocks.Add(mainSection);

            HtmlNode mainNode = htmlDoc.DocumentNode;
            Html_To_Flow(mainNode, mainSpan);

            return flowDoc;
        }

        void Html_To_Flow(HtmlNode node, Span parent)
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


        bool IsAttributeOfInterest(HtmlAttribute attr)
        {
            HtmlNode node = attr.OwnerNode;
            string nodeName = node.Name.ToLower().Trim();
            string attrName = attr.Name.ToLower().Trim();

            switch (nodeName)
            {
                /*case "a":
                {
                    switch (attrName)
                    {
                        case "href":
                            return true;
                    }
                    break;
                }*/
                case "img":
                    {
                        switch (attrName)
                        {
                            case "src":
                                return true;
                        }
                        break;
                    }
            }

            return false;
        }

        void HtmlAttribute_To_Runs(HtmlAttribute attr, Span parent)
        {
            Run r1 = new Run(string.Format(" {0}=\"", attr.OriginalName));
            parent.Inlines.Add(r1);

            if (string.IsNullOrEmpty(attr.Value) == false)
            {
                Hyperlink r2 = AddHyperlink(attr.Value, attr);
                r2.Foreground = defaultColor;
                parent.Inlines.Add(r2);
                //htmlToTextElement.Add(attr, r2);

                if (IsAttributeOfInterest(attr))
                {
                    r2.Foreground = attributeColor;
                    r2.FontSize = attributeFontSize;
                }
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
                hlink.Foreground = defaultColor;
                //htmlToTextElement.Add(node, hlink);

                if (string.Compare(node.ParentNode.Name, "script", true) == 0)
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
                }
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

        protected override TreeViewItem CreateSourceTree()
        {
            HtmlNode rootHtmlNode = htmlDoc.DocumentNode;
            rootHtmlNode = rootHtmlNode.SelectSingleNode("/html");
            TreeViewItem root = CreateTvi(rootHtmlNode);
            
            //Html_To_TreeViewItem(rootHtmlNode, root);
            foreach (HtmlNode child in rootHtmlNode.ChildNodes)
            {
                Html_To_TreeViewItem(child, root);
            }

            root.ExpandSubtree();

            return root;
        }


        private TreeViewItem CreateTvi(HtmlNode node)
        {

            /*string xpath = node.XPath;
            string[] xpathParts = xpath.Split(new char[] { '/' });
            string lastPart = xpathParts[xpathParts.Length - 1];*/
            HtmlNodeCollection siblings = node.SelectNodes("../" + node.Name);

            StringBuilder sb = new StringBuilder();
            sb.Append(node.Name);
            if( siblings.Count>1 )
            {
                sb.AppendFormat("[{0}]", siblings.IndexOf(node));
            }

            TreeViewItem tvi = new TreeViewItem();
            tvi.Header = sb.ToString();           
            return tvi;
        }

        void Html_To_TreeViewItem(HtmlNode node, TreeViewItem parent)
        {
            /*switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                case HtmlNodeType.Text:
                    HtmlText_To_TreeViewItem(node, parent);
                    return;
            }


            if (node.NodeType != HtmlNodeType.Document)
            {

                // ATTRIBUTES ?
                if (node.HasAttributes)
                {
                    foreach (HtmlAttribute attr in node.Attributes)
                    {
                        HtmlAttribute_To_TreeViewitem(attr, parent);
                    }
                }
            }
            */

            /*string xpath = node.XPath;
            string[] xpathParts = xpath.Split(new char[] { '/' });
            string lastPart = xpathParts[xpathParts.Length - 1];
            TreeViewItem tvi = new TreeViewItem();
            tvi.Header = lastPart;*/

            if (node.NodeType != HtmlNodeType.Element)
                return;

            TreeViewItem tvi = CreateTvi(node);
            parent.Items.Add(tvi);

            // PROCESS CHILDREN
            foreach (HtmlNode child in node.ChildNodes)
            {
                Html_To_TreeViewItem(child, tvi);
            }



        }


        /*
        private TreeViewItem CreateTreeNode(HtmlNode htmlNode)
        {
            Span header = CreateTreeNodeHeader(htmlNode);
            if ((header == null) || (header.Inlines.Count == 0))
            {
                return null;
            }
        
            TreeViewItem treeNode = AddTreeNode(header, htmlNode);

            foreach(HtmlNode htmlChildNode in htmlNode.ChildNodes)
            {
                TreeViewItem childNode = CreateTreeNode(htmlChildNode);
                if (childNode != null)
                {
                    treeNode.Items.Add(childNode);
                }
            }

            return treeNode;
        }

        private Span CreateTreeNodeHeader(HtmlNode htmlNode)
        {
            Span header = new Span();

            switch (htmlNode.NodeType)
            {
                case HtmlNodeType.Comment:
                case HtmlNodeType.Text:
                    if( string.IsNullOrEmpty(htmlNode.InnerText)==false )
                    {
                        header.Inlines.Add(htmlNode.InnerText);
                    }
                    break;

                default:
                    header.Inlines.Add(htmlNode.OriginalName);
                    break;
            }

            foreach(HtmlAttribute attribute in htmlNode.Attributes)
            {
                string attr = string.Format("{0}={1}", attribute.OriginalName, attribute.Value);
                header.Inlines.Add(attr);
            }

            return header;
        }
        */

        public override XPathNavigator CreateNavigator()
        {
            return htmlDoc.CreateNavigator();
        }
    }
}
