using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    class HtmlXPathBuilder:AbstractXPathBuilder
    {
        private readonly HtmlNode documentNode;

        public HtmlXPathBuilder(HtmlNode documentNode)
        {
            this.documentNode = documentNode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">un objet de type HtmlNode ou HtmlAttribute, et qui se trouve
        /// quelque part dans l'arboresence de documentNode.</param>
        /// <seealso cref="documentNode"/>
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
            HideFinalTextElement();
        }


        /// <summary>
        /// Si le dernier élément du XPath est un noeud text(),
        /// on peut le cacher.
        /// </summary>
        private void HideFinalTextElement()
        {
            if (Elements.Count < 1)
                return;

            if (Elements.Last() is HtmlNodeXPathElement last)
            {
                if (last.HtmlNode.NodeType == HtmlNodeType.Text)
                {
                    last.Visible = false;
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
                    Attributes.Add(new XPathAttribute(attr.Name, attr.Value));
                }
            }

            public override ICollection<string> SingularAttributeNames
            {
                get { return new string[] { "id" }.ToList(); }
            }

            public override ICollection<string> ImportantAttributeNames
            {
                get
                {
                    List<string> attributes = new List<string>();
                    attributes.Add("rel");
                    attributes.Add("class");
                    return attributes;
                }
            }

            protected override string ElementName
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

            public override bool CanBeMisguiding(XPathAttribute attribute)
            {
                switch(attribute.Name)
                {
                    case "class":
                        return Regex.Split(attribute.Value, @"\s").Length > 1;

                    default:
                        return false;
                }
            }
        }

        internal class HtmlAttributeXPathElement:XPathElement
        {
            internal readonly HtmlAttribute HtmlAttribute;

            internal HtmlAttributeXPathElement(HtmlAttribute attribute)
            {
                this.HtmlAttribute = attribute;
            }

            public override ICollection<string> SingularAttributeNames
            {
                get { /*return new string[] {}.ToList();*/ throw new NotImplementedException(); }
            }

            public override ICollection<string> ImportantAttributeNames => throw new NotImplementedException();

            protected override string ElementName
            {
                get
                {
                    return "@"+HtmlAttribute.Name;
                }
            }

            public override bool CanBeMisguiding(XPathAttribute attribute)
            {
                throw new NotImplementedException();
            }
        }
    }
}
