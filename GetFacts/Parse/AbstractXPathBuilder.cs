using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    public abstract class AbstractXPathBuilder
    {
        private readonly List<XPathElement> elements = new List<XPathElement>();

        public abstract void Build(object o);

        protected void Add(XPathElement element)
        {
            elements.Add(element);
        }

        public string GetString()
        {
            StringBuilder result = new StringBuilder();

            foreach(XPathElement element in elements)
            {
                if(element.Visible==false )
                {
                    continue;
                }

                result.Append(SeparatorFor(element));
                result.Append(element.GetString());
            }

            return result.ToString();
        }

        private string SeparatorFor(XPathElement currentElement)
        {
            int index = elements.IndexOf(currentElement);
            if( index>0 )
            {
                XPathElement previousElement = elements[index - 1];
                if( previousElement.Visible==false )
                {
                    return "//";
                }
            }
            return "/";
        }

        /// <summary>
        /// Altère la visibilité des XPathElement's qui composent ce 'XPathBuilder,
        /// de façon à former une expression à la fois plus synthétique et plus
        /// inclusive.
        /// </summary>
        /// <see cref="HideAllElementsBeforeTheSingularAttributeClosestToTheObject"/>
        /// <see cref="ShowAllImportantAttributesForEachElement"/>
        internal void Optimize()
        {
            HideAllElementsBeforeTheSingularAttributeClosestToTheObject();
            ShowAllImportantAttributesForEachElement();
        }


        /// <summary>
        /// Tente de simplifier l'expression en supprimant tout ce qui est à gauche
        /// d'un noeud unique.
        /// </summary>
        /// <example>
        /// Soit le code HTML suivant: 
        /// <code>
        /// <html><head><title>TITRE</title></head><body><div id="tag1"><p>Texte</p></div></body></html>
        /// </code>
        /// L'expression complete pour atteindre "Texte", avant optimisation, est:
        /// <code>
        /// /html/body/div/p
        /// </code>
        /// Après optimisation, l'expression est:
        /// <code>
        /// //div[@id="tag1"]/p
        /// </code>
        /// </example>
        /// <see cref="XPathElement.SingularAttributeNames"/>
        private void HideAllElementsBeforeTheSingularAttributeClosestToTheObject()
        {
            int nbOfElements = elements.Count;
            int index = nbOfElements - 1;

            while (index >= 0)
            {
                XPathElement element = elements[index--];
                var singularAttributes = from attribute in element.Attributes where element.SingularAttributeNames.Contains(attribute.Name) == true select attribute;
                if( singularAttributes.Count() > 0 )
                {
                    singularAttributes.ElementAt(0).Visible = true;
                    break;
                }
            }

            while(index>=0)
            {
                XPathElement element = elements[index--];
                element.Visible = false;
            }


        }

        private void ShowAllImportantAttributesForEachElement()
        {
            foreach(XPathElement element in elements)
            {
                var singularAttributes = from a in element.Attributes where element.SingularAttributeNames.Contains(a.Name) && a.Visible select a;
                if (singularAttributes.Any())
                    continue;

                var importantAttributes = from a in element.Attributes where element.ImportantAttributeNames.Contains(a.Name) select a;
                foreach (var importantAttribute in importantAttributes)
                {
                    if( element.CanBeMisguiding(importantAttribute)== false )
                        importantAttribute.Visible = true;
                }
            }
        }

        public abstract class XPathElement
        {
            internal readonly List<XPathAttribute> Attributes = new List<XPathAttribute>();

            /// <summary>
            /// Noms des attributs qui, pour la technologie considérée (Xml, Html, ...),
            /// sont des "singularités". C'est-à-dire, des attributs qui permettent d'identifier
            /// de façon unique un élément dans l'arboresence du document. 
            /// </summary>
            /// <example>
            /// l'attribut "id" pour le HTML.
            /// </example>
            public abstract ICollection<string> SingularAttributeNames { get; }

            /// <summary>
            /// Noms des attributs qui, pour la technologie considérée (Xml, Html, ...),
            /// sont des facilitateurs pour localiser du contenu dans l'arboresence du document.
            /// </summary>
            /// <example>
            /// l'attribut "class" pour le HTML.
            /// </example>
            public abstract ICollection<string> ImportantAttributeNames { get; }

            protected abstract string ElementName { get; }

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
        }

        public class XPathAttribute
        {
            public XPathAttribute(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; private set; }
            public string Value { get; private set; }

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
                if(withValue)
                {
                    sb.AppendFormat("=\"{0}\"", Value);
                }
                return sb.ToString();
            }
        }
    }
}
