using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    public class XmlParser:AbstractParser
    {
        private XmlDocument xmlDoc = null;

        public XmlParser()
        {
            xmlDoc = new XmlDocument();
        }

        /// <summary>
        /// Lit et charge dans un HtmlDocument les données contenues dans le fichier
        /// HTML spécifié par le paramètre "path".
        /// </summary>
        /// <param name="path">Le chemin absolu du fichier contenant les données à analyser</param>
        /// <param name="encoding">Si null, l'encoding sera déterminé
        /// automatiquement</param>
        public override void Load(string path, Encoding encoding)
        {
            Clear();

            using (Stream streamReader = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                TextReader textReader;
                if (encoding != null)
                {              
                    textReader = new StreamReader(streamReader, encoding);
                }
                else
                {
                    textReader = new StreamReader(streamReader, true);
                }

                try
                {
                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    using (XmlReader reader = XmlReader.Create(textReader, readerSettings))
                    {
                        xmlDoc.Load(reader);
                    }
                }
                finally
                {
                    textReader.Dispose();
                }
            }
        }

        /// <summary>
        /// Retourne une liste des extensions de fichier
        /// qui sont le plus couramment associées au
        /// format XML.
        /// </summary>
        /// <remarks>Cette instace retournera {".xml"}</remarks>        
        public override string[] UsualFileExtensions
        {
            get { return new string[] { ".xml" }; }
        }

        #region styling

        /// <summary>
        /// Implémentation concrète de AbstractParser.EvaluateInformationType.
        /// Si o est de type XmlElement, renvoie la valeur de EvaluateInformationype(XmlElement).
        /// Si o est de type XmlAttribute, renvoie la valeur de EvaluateInformationType(XmlAttribute)
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <see cref="AbstractParser.EvaluateInformationType(object)"/>
        protected override InformationType EvaluateInformationType(object o)
        {
            /*if (o is XmlElement)
            {
                return EvaluateInformationType((XmlElement)o);
            }
            else if (o is XmlAttribute)
            {
                return EvaluateInformationType((XmlAttribute)o);
            }*/
            return InformationType.NeutralData;
        }

        // TODO
        /*private InformationType EvaluateInformationType(XmlElement node)
        {
            return InformationType.NeutralData;
        }*/

        // TODO
        /*private InformationType EvaluateInformationType(XmlAttribute attr)
        {
            return InformationType.NeutralData;
        }*/

        #endregion


        #region flow document

        protected override void FillSourceCode(Span rootSpan)
        {
            foreach(XmlNode node in xmlDoc.ChildNodes)
            {
                XmlNode_To_Flow(node, rootSpan);
            }
        }

        /// <summary>
        /// Opération récursive qui parcourt l'arbre démarrant à 'node',
        /// et crée dans 'parent' un dérivé de TextElement approprié.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        /// <seealso cref=" XmlNode_To_TreeView"/>
        private void XmlNode_To_Flow(XmlNode node, Span parent)
        {
            // Les types de noeuds suivants sont des feuilles
            // de l'arborescence et peuvent être rendus "à plat":
            switch (node.NodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.XmlDeclaration:
                    InsertText(parent, node.OuterXml, node);
                    return;

                case XmlNodeType.Text:
                    InsertText(parent, node.InnerText, node);
                    return;
            }

            if (node.NodeType == XmlNodeType.Element)
            {
                Span globalSpan = new Span();
                parent.Inlines.Add(globalSpan);
                Stylize(globalSpan, node);

                // XML TAG (opening)
                Run openingTag = new Run();
                openingTag.Text = string.Format("<{0}", node.Name);
                globalSpan.Inlines.Add(openingTag);

                // ATTRIBUTES ?
                if (node.Attributes.Count>0 )
                {
                    foreach (XmlAttribute attr in node.Attributes)
                    {
                        XmlAttribute_To_Runs(attr, globalSpan);
                    }
                }

                // XML TAG (close opening tag)
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
            else
            {
            }

            // PROCESS CHILDREN
            foreach (XmlNode child in node.ChildNodes)
            {
                XmlNode_To_Flow(child, parent);
            }


            // XML TAG (closing)
            if ((node.NodeType == XmlNodeType.Element) && (node.HasChildNodes == true))
            {
                Run closingTag = new Run(string.Format("</{0}>", node.Name));
                parent.Inlines.Add(closingTag);
            }
        }


        void XmlAttribute_To_Runs(XmlAttribute attr, Span parent)
        {
            Run r1 = new Run(string.Format(" {0}=\"", attr.Name));
            parent.Inlines.Add(r1);

            if (string.IsNullOrEmpty(attr.Value) == false)
            {
                Hyperlink r2 = AddHyperlink(attr.Value, attr);
                parent.Inlines.Add(r2);
            }

            Run r3 = new Run("\"");
            parent.Inlines.Add(r3);
        }

        #endregion


        #region tree

        protected override TreeViewItem CreateSourceTree()
        {
            XmlElement rootXmlNode = xmlDoc.DocumentElement;
            TreeViewItem root = XmlNode_To_TreeViewItem(rootXmlNode);

            foreach (XmlNode child in rootXmlNode.ChildNodes)
            {
                XmlNode_To_TreeView(child, root);
            }

            return root;
        }

        /// <summary>
        /// Opération récursive qui parcourt l'arbre démarrant à 'node',
        /// et crée dans 'parent' un TreeViewItem approprié.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        /// <seealso cref="XmlNode_To_Flow"/>
        void XmlNode_To_TreeView(XmlNode node, TreeViewItem parent)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                TreeViewItem tvi = XmlNode_To_TreeViewItem(node);
                parent.Items.Add(tvi);

                // PROCESS CHILDREN
                foreach (XmlNode child in node.ChildNodes)
                {
                    XmlNode_To_TreeView(child, tvi);
                }
            }

            else if (node.NodeType == XmlNodeType.Text)
            {
                TreeViewItem tvi = Text_To_TreeViewItem(node.OuterXml, node);
                if (tvi != null)
                {
                    parent.Items.Add(tvi);
                }
            }

        }

        /// <summary>
        /// Crée un TreeViewItem qui permet de rendre le contenu
        /// du XmlNode passé en argument.
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        private TreeViewItem XmlNode_To_TreeViewItem(XmlNode xmlNode)
        {
            Run header = new Run(xmlNode.Name);
            TreeViewItem nodeItem = AddTreeNode(header, xmlNode);

            // Attributs du noeud:
            foreach (XmlAttribute attributeNode in xmlNode.Attributes)
            {
                Run attribute = new Run(
                    string.Format("@{0} = \"{1}\"",
                        attributeNode.Name,
                        attributeNode.Value));
                TreeViewItem attributeItem = AddTreeNode(attribute, attributeNode);
                nodeItem.Items.Add(attributeItem);
            }

            return nodeItem;
        }

        #endregion


        #region xpath

        public override XPathNavigator CreateNavigator()
        {
            return xmlDoc.CreateNavigator();
        }

        protected override AbstractXPathBuilder XPathFor(object o)
        {
            XmlXPathBuilder builder = new XmlXPathBuilder(xmlDoc.DocumentElement);
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

            foreach (XPathNavigator navigator in navigators)
            {
                XPathNodeIterator selection = navigator.Select(xpath);
                foreach (XPathNavigator selected in selection)
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

                foreach (string xpath in xpathElements)
                {
                    if (!string.IsNullOrEmpty(xpath))
                    {
                        navigators = SubSelect(navigators, xpath);
                    }
                }

                foreach (XPathNavigator selected in navigators)
                {
                    /*if (selected is HtmlNodeNavigator nodeNav)
                    {
                        HtmlNode concreteNode = nodeNav.CurrentNode;
                        output.Add(concreteNode);
                    }
                    else
                    {

                    }*/
                }
            }

            return output;
        }



        #endregion
    }
}
