using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    [DebuggerDisplay("XPathAttribute {Name}={Value}")]
    public abstract class XPathAttribute
    {
        public XPathAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }

        /// <summary>
        /// Indique si l'élément, pour la technologie considérée (Xml, Html, ...),
        /// est une "singularité". C'est-à-dire, une composante de l'expressions XPath 
        /// qui permettent d'identifier de façon unique un élément dans l'arboresence du document.
        /// </summary>
        /// <example>
        /// l'attribut "id" pour le HTML.
        /// </example>
        public abstract bool IsSingular { get; }


        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            internal set { visible = value; }
        }

        public string GetString()
        {
            return GetString(false);
        }

        public string GetString(bool withValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("@{0}", Name);
            if (withValue)
            {
                sb.AppendFormat("=\"{0}\"", Value);
            }
            return sb.ToString();
        }
    }    
}
