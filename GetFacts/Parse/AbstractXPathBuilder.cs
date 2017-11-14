using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    /// <summary>
    /// Cette classe encapsule une liste ordonnée d'objets XPathElement
    /// (chaque XPathElement représente un morceau de l'expression XPath
    /// totale représentée par AbstractXPathBuilder).
    /// L'action se déroule en deux temps:
    /// 1- On trouve un chemin dans le document analysé qui permettent d'atteindre
    ///    la cible de l'expression XPath à construire. Un XPathElement est instancié
    ///    pour chaque "noeud" de ce chemin.
    /// 2- On essaye de simplifier l'expression résultante en masquant les informations non
    ///    pertinentes et en affichant les informations vraiment utiles.
    /// </summary>
    /// <see cref="Build(object)"/>
    /// <see cref="Optimize"/>
    /// <remarks>
    /// Pour implémenter concrètement cette classe abstraite:
    /// - Une référence au document servant de base à l'analyse doit être incorporée.
    ///   Par exemple, le constructeur de XmlXPathBuilder prend un paramètre le XmlElement qui est la racine du document.
    /// - La fonction Build() doit être implémentée. Durant le Build, on appel Add(XPathElement) pour constuire l'expression XPath représentée par cet objet.
    /// - La fonction Optimize() peut, si nécessaire, être héritée.
    /// </remarks>
    public abstract class AbstractXPathBuilder
    {
        private List<XPathElement> elements = null;
        
        public bool? IsAbsolute
        {
            get;
            private set;
        }

        protected IReadOnlyList<XPathElement> Elements
        {
            get
            {
                if (elements == null) return null;
                return elements.AsReadOnly();
            }
        }

        /// <summary>
        /// Initialise la liste Elements avec des instances 
        /// d'XPathElement, construisant un chemin partant de la racine
        /// du document et permettant d'atteindre le noeud correspondant
        /// à l'objet concret "target".
        /// </summary>
        /// <param name="target"></param>
        /// <example>Pour HtmlXPathBuilder, "target" sera un HtmlNode ou un HtmlAttribute du HtmlDocument étudié.</example>
        /// <remarks>Cette méthode délègue le travail à la méthode abstraite BuildImpl()</remarks>
        /// <seealso cref="BuildImpl(object)"/>
        public void Build(object target)
        {
            if (elements != null) throw new Exception();

            elements = new List<XPathElement>();
            IsAbsolute = true;
            BuildImpl(target);
        }

        protected abstract void BuildImpl(object target);

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
        internal virtual void Optimize()
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
