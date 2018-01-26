using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    [DebuggerDisplay("XPathAttribute {Name}={Value}")]
    public abstract class XPathAttribute:IComparable<XPathAttribute>
    {
        public XPathAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; internal set; }

        /// <summary>
        /// Indique si l'élément, pour la technologie considérée (Xml, Html, ...),
        /// est une "singularité". C'est-à-dire, une composante de l'expressions XPath 
        /// qui permettent d'identifier de façon unique un élément dans l'arboresence du document.
        /// </summary>
        /// <example>
        /// l'attribut "id" pour le HTML.
        /// </example>
        public abstract bool IsSingular { get; }

        /// <summary>
        /// Indique si l'élément, pour la technologie considérée (Xml, Html, ...),
        /// est un facilitateur pour localiser du contenu dans l'arboresence du document.
        /// </summary>
        /// <example>
        /// l'attribut "class" pour le HTML.
        /// </example>
        public abstract bool IsImportant { get; }

        /// <summary>
        /// Appréciation de la fiabilité de l'attribut, pour savoir
        /// s'il derait être utilisé ou pas pour l'optimisation
        /// d'un expression.
        /// </summary>
        /// <remarks>Retourne "true" si quelque chose dans le contexte du document ou dans la valeur
        /// de l'attribut laise penser qu'il sera difficile d'en faire usage pour le XPath
        /// en cours d'optimisation. Sinon, retourne "false".</returns>
        /// </remarks>
        /// Dans le cas du HTML, un attribut "class" dont la valeur contient plusieurs mots séparés
        /// par des espaces retournera "true", car non fiable.
        /// </example>
        public abstract bool CanBeMisguiding { get; }



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
                if (!string.IsNullOrEmpty(Value))
                {
                    sb.AppendFormat("=\"{0}\"", Value);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// 0 --> égalité
        /// -1 --> différence de valeur pour Name
        /// 1 --> différence de valeur pour Value
        /// </returns>
        public int CompareTo(XPathAttribute a)
        {
            if (string.Compare(this.Name, a.Name) != 0)
                return -1;
            if (string.Compare(this.Value, a.Value) != 0)
                return 1;
            return 0;
        }
    }    
}
