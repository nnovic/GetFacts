using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    class HtmlParser:AbstractParser
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

        protected override void ClearSourceCode()
        {
            base.ClearSourceCode();
            htmlToTextElement.Clear();
        }

        public override void Clear()
        {
            base.Clear();
            if( htmlDoc!=null)
            {
                // TODO: Clear ??
                // New ??
            }
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

        private Hashtable htmlToTextElement = new Hashtable();

        protected override TextElement GetTextElementAssociatedWith(object o)
        {
            return (TextElement)htmlToTextElement[o];
        }


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
                htmlToTextElement.Add(node, globalSpan);

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
                htmlToTextElement.Add(attr, r2);

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
                htmlToTextElement.Add(node, hlink);

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

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o">Can be an instance of HtmlNode or HtmlAttribute</param>
        protected override void UpdateCodeTree(object o, out TreeViewItem leaf)
        {
            HtmlNode node =null;
            TreeViewItem child = null;
            leaf = null;

            if (o is HtmlNode)
            {
                node = (HtmlNode)o;
            }
            else if(o is HtmlAttribute)
            {                
                child = new TreeViewItem();
                child.Tag = o;
                HtmlAttribute attr = (HtmlAttribute)o;
                child.Header = string.Format("{0} = \"{1}\"", attr.Name, attr.Value);
                node = attr.OwnerNode;
            }
            else
            {
                throw new ArgumentException();
            }

            while( (node != null) && (node.NodeType!= HtmlNodeType.Document) )
            {
                if (leaf == null) leaf = child;

                string idValue = node.GetAttributeValue("id", (string)null);
                string classValue = node.GetAttributeValue("class", (string)null);

                TreeViewItem parent = null;
                if (node.ParentNode.NodeType == HtmlNodeType.Document)
                {
                    parent = SelectedSource;
                }
                else
                {
                    parent = new TreeViewItem();
                }
                parent.Tag = node;

                StringBuilder sb = new StringBuilder();
                sb.Append(node.Name);
                if (idValue != null) sb.AppendFormat(" id=\"{0}\"", idValue);
                if( classValue != null ) sb.AppendFormat(" class=\"{0}\"", classValue);
                parent.Header = sb.ToString();


                if( child != null )
                {
                    parent.Items.Add(child);
                }

                child = parent;
                node = node.ParentNode;
            }
        }

        #region tree view

        /// <summary>
        /// Returns the XPath that best represents the object provided
        /// in arguments, regarding its place in the hierarchy of the
        /// source code.
        /// </summary>
        /// <param name="o">An instance of HtmlNode or HtmlAttribure</param>
        /// <returns></returns>
        protected override string GetConcreteXPathOf(object o)
        {
            if( o is HtmlNode )
            {
                HtmlNode node = (HtmlNode)o;
                return node.XPath;
            }
            else if ( o is HtmlAttribute )
            {
                HtmlAttribute a = (HtmlAttribute)o;
                return a.XPath;
            }
            throw new ArgumentException();
        }

        #endregion

        public override XPathNavigator CreateNavigator()
        {
            return htmlDoc.CreateNavigator();
        }
    }
}
