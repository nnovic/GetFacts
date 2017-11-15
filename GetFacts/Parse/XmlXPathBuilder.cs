using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace GetFacts.Parse
{
    public class XmlXPathBuilder:AbstractXPathBuilder
    {
        public XmlXPathBuilder()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">Un objet XmlElement ou XmlAttribute, qui se
        /// trouve quelque part dans l'arboresence du document XML.</param>
        /// <seealso cref="documentNode"/>
        protected override void BuildImpl(object target)
        {
            if (target is XmlElement node)
            {
                Build(node);
            }
            else if (target is XmlAttribute attr)
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

        private void Build(XmlElement node)
        {
            // construire la hiérarchie de l'objet du bas vers le haut:
            List<XmlNode> hierarchy = new List<XmlNode>();
            XmlNode o = node;
            while( o != null )
            {
                hierarchy.Add(o);
                o = o.ParentNode;
            }

            hierarchy.Reverse(); // mettre le haut (la racine) de la hiérarchie en premier

            foreach (XmlNode o2 in hierarchy)
            {
                if (o2.NodeType == XmlNodeType.Document)
                    continue;

                XmlNodeXPathElement element = new XmlNodeXPathElement(o2);
                Add(element);
            }
        }

        internal class XmlNodeXPathElement:XPathElement
        {
            internal readonly XmlNode XmlNode;

            internal XmlNodeXPathElement(XmlNode node)
            {
                this.XmlNode = node;
                foreach(XmlAttribute attr in node.Attributes)
                {
                    Attributes.Add(new XmlXPathAttribute(attr.Name, attr.Value));
                }
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
                    return XmlNode.Name;
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

        internal class XmlXPathAttribute:XPathAttribute
        {
            public XmlXPathAttribute(string name, string value)
                : base(name, value)
            {
            }

            public override bool IsSingular
            {
                get
                {
                    return false;
                }
            }
        }
    }
}
