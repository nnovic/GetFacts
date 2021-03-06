﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    public class HtmlXPathBuilder:AbstractXPathBuilder
    {
        private static readonly string[] HtmlSingularAttributeNames = new string[] { "id" };
        private static readonly string[] HtmlImporantAttributeNames = new string[] { "rel", "class" };


        public HtmlXPathBuilder()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">un objet de type HtmlNode ou HtmlAttribute, et qui se trouve
        /// quelque part dans l'arboresence du document HTMl.</param>
        protected override void BuildImpl(object target)
        {
            if (target is HtmlNode node)
            {
                Build(node);
            }
            else if (target is HtmlAttribute attr)
            {
                HtmlNode parent = attr.OwnerNode;
                Build(parent);
                Add(new HtmlAttributeXPathElement(attr));
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private void Build(HtmlNode node)
        {
            List<HtmlNode> hierarchy = new List<HtmlNode>(node.AncestorsAndSelf());
            hierarchy.Reverse(); // mettre le haut (la racine) de la hiérarchie en premier
            foreach(HtmlNode o in hierarchy)
            {
                if (o.NodeType == HtmlNodeType.Document)
                    continue;

                HtmlNodeXPathElement element = new HtmlNodeXPathElement(o);
                Add(element);
            }
        }

        internal override void Optimize()
        {
            base.Optimize();
            DumpFinalTextElement();
        }


        /// <summary>
        /// Si le dernier élément du XPath est un noeud text(),
        /// on peut l'ignorer complètement.
        /// </summary>
        private void DumpFinalTextElement()
        {
            if (Elements.Count < 1)
                return;

            if (Elements.Last() is HtmlNodeXPathElement last)
            {
                if (last.HtmlNode.NodeType == HtmlNodeType.Text)
                {
                    last.Dumped = true;
                }
            }
        }

        internal class HtmlNodeXPathElement:XPathElement
        {
            internal readonly HtmlNode HtmlNode;

            internal HtmlNodeXPathElement(HtmlNode node)
            {
                this.HtmlNode = node;
                foreach(HtmlAttribute attr in node.Attributes)
                {
                    Attributes.Add(new HtmlXPathAttribute(attr.Name, attr.Value));
                }
            }

            public override string ElementName
            {
                get
                {
                    switch (HtmlNode.NodeType)
                    {
                        case HtmlNodeType.Comment: return "comment()";
                        case HtmlNodeType.Text: return "text()";
                    }
                    return HtmlNode.Name;
                }
            }

            protected override object ConcreteElement => this.HtmlNode;
        }

        internal class HtmlAttributeXPathElement:XPathElement
        {
            internal readonly HtmlAttribute HtmlAttribute;

            internal HtmlAttributeXPathElement(HtmlAttribute attribute)
            {
                this.HtmlAttribute = attribute;
            }

            public override string ElementName =>"@"+this.HtmlAttribute.Name;

            protected override object ConcreteElement => this.HtmlAttribute;
        }

        internal class HtmlXPathAttribute : XPathAttribute
        {
            public HtmlXPathAttribute(string name, string value) 
                : base(name, value)
            {
            }

            public override bool IsSingular
            {
                get
                {
                    return HtmlSingularAttributeNames.Contains(Name);
                }
            }

            public override bool IsImportant
            {
                get
                {
                    return HtmlImporantAttributeNames.Contains(Name);
                }
            }

            public override bool CanBeMisguiding
            {
                get
                {
                    switch (Name)
                    {
                        case "class":
                            return Regex.Split(Value, @"\s").Length > 1;

                        default:
                            return false;
                    }
                }
            }
        }
    }
}
