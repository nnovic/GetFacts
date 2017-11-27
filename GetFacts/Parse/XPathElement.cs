using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    [DebuggerDisplay("XPathElement = {ElementName}")]
    public abstract class XPathElement : IEquatable<XPathElement>
    {
        internal readonly List<XPathAttribute> Attributes = new List<XPathAttribute>();

        protected abstract string ElementName { get; }

        protected abstract object ConcreteElement { get; }

        public bool HasAnyVisibleAttribute
        {
            get
            {
                var visibleAttributes = from a in Attributes where a.Visible == true select a;
                return visibleAttributes.Any();
            }
        }

        private string VisibleAttributes(string separator)
        {
            var visibleAttributes = from a in Attributes where a.Visible == true select a;
            StringBuilder sb = new StringBuilder();
            bool isFirstAttribute = true;
            foreach (var attribute in visibleAttributes)
            {
                if (isFirstAttribute == true)
                    isFirstAttribute = false;
                else
                    sb.Append(separator);
                sb.Append(attribute.GetString(true));
            }
            return sb.ToString();
        }

        public string GetString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ElementName);

            if (HasAnyVisibleAttribute)
            {
                sb.Append('[');
                sb.Append(VisibleAttributes(" and "));
                sb.Append(']');
            }

            return sb.ToString();
        }

        private bool visible = true;
        public bool Visible
        {
            get { return visible; }
            internal set { visible = value; }
        }

        /// <summary>
        /// Evalue la fiabilité de l'attribut passé en argument pour
        /// devenir un élément du XPath après optmisation.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns>Retourne "true" si quelque chose dans le contexte du document ou dans la valeur
        /// de l'attribut laise penser qu'il sera difficile d'en faire usage pour le XPath
        /// en cours d'optimisation. Sinon, retourne "false".</returns>
        /// <example>
        /// Dans le cas du HTML, un attribut "class" dont la valeur contient plusieurs mots séparés
        /// par des espaces retournera "true", car non fiable.
        /// </example>
        public abstract bool CanBeMisguiding(XPathAttribute attribute);



        public virtual bool Equals(XPathElement other)
        {
            return ConcreteElement.Equals(other.ConcreteElement);
        }
        

        public class GoBackElement : XPathElement
        {
            protected override string ElementName => "..";

            protected override object ConcreteElement => null;

            public override bool CanBeMisguiding(XPathAttribute attribute)
            {
                throw new NotImplementedException();
            }

            public override bool Equals(XPathElement other)
            {
                return other is GoBackElement;
            }
        }
    }
}
