using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace GetFacts.Parse
{
    class XmlXPathBuilder:AbstractXPathBuilder
    {
        private readonly XmlElement documentNode;

        public XmlXPathBuilder(XmlElement documentNode)
        {
            this.documentNode = documentNode;
        }

        public override void Build(object o)
        {
            if (o is XmlElement node)
            {
                Build(node);
            }
            else if (o is XmlAttribute attr)
            {
                XmlElement parent = attr.OwnerElement;
                Build(parent);
                Add(new XmlAttributeXPathElement(attr));
            }
            else
            {
                throw new ArgumentException();
            }
        }

        // TODO
        private void Build(XmlElement node)
        {
            /*List<XmlElement> hierarchy = new List<XmlElement>(node.AncestorsAndSelf());
            hierarchy.Reverse();
            foreach(HtmlNode o in hierarchy)
            {
                if (o.NodeType == HtmlNodeType.Document)
                    continue;

                HtmlNodeXPathElement element = new HtmlNodeXPathElement(o);
                Add(element);
            }*/
        }

        internal class XmlElementXPathElement:XPathElement
        {
            internal readonly XmlElement XmlElement;

            internal XmlElementXPathElement(XmlElement node)
            {
                this.XmlElement = node;
                foreach(XmlAttribute attr in node.Attributes)
                {
                    Attributes.Add(new XPathAttribute(attr.Name, attr.Value));
                }
            }

            // TODO
            public override ICollection<string> SingularAttributeNames
            {
                get { return new string[] { /*"id"*/ }.ToList(); }
            }

            // TODO
            public override ICollection<string> ImportantAttributeNames
            {
                get
                {
                    List<string> attributes = new List<string>();
                    //attributes.Add("class");
                    return attributes;
                }
            }

            protected override string ElementName
            {
                get
                {
                    // TODO
                    /*switch (XmlElement.NodeType)
                    {
                        case HtmlNodeType.Comment: return "comment()";
                        case HtmlNodeType.Text: return "text()";
                    }*/
                    return XmlElement.Name;
                }
            }

            public override bool CanBeMisguiding(XPathAttribute attribute)
            {
                switch(attribute.Name)
                {
                    // TODO
                    //case "class":
                    //    return Regex.Split(attribute.Value, @"\s").Length > 1;

                    default:
                        return false;
                }
            }
        }

        internal class XmlAttributeXPathElement:XPathElement
        {
            internal readonly XmlAttribute XmlAttribute;

            internal XmlAttributeXPathElement(XmlAttribute attribute)
            {
                this.XmlAttribute = attribute;
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
                    return "@"+XmlAttribute.Name;
                }
            }

            public override bool CanBeMisguiding(XPathAttribute attribute)
            {
                throw new NotImplementedException();
            }
        }
    }
}
