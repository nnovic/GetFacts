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
    public abstract class AbstractXPathBuilder:IEquatable<AbstractXPathBuilder>
    {
        private List<XPathElement> elements = null;
        
        public bool? IsAbsolute
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne, dans une liste non-modifiable, la liste
        /// des XPathElement qui composent le XPath représenté
        /// par cet objet.
        /// </summary>
        public IReadOnlyList<XPathElement> Elements
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


        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            if (IsAbsolute == false)
                result.Append('.');

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
        /// <see cref="HideAllElementsBeforeTheSingularAttributeClosestToTheTarget"/>
        /// <see cref="ShowAllImportantAndUnambiguousAttributesForEachElement"/>
        /// <see cref="ShowImportantButMisguidingAttributeForTheLastElement"/>
        internal virtual void Optimize()
        {
            HideAllElementsBeforeTheSingularAttributeClosestToTheTarget();
            if (ShowAllImportantAndUnambiguousAttributesForEachElement() == false)
            {
                ShowImportantButMisguidingAttributeForTheLastElement();
            }
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
        private void HideAllElementsBeforeTheSingularAttributeClosestToTheTarget()
        {
            int nbOfElements = elements.Count;
            int index = nbOfElements - 1;

            while (index >= 0)
            {
                XPathElement element = elements[index--];
                var singularAttributes = from attribute in element.Attributes where attribute.IsSingular == true select attribute;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>'true' --> au moins un argument important non ambigu a été trouvé et rendu visible.
        /// 'false' --> aucun arugment n'a été trouvé qui soit en même temps 'important' et 'non ambigu'.
        /// </returns>
        /// <remarks>
        /// Attention, un attribut avec IsImportant=true, mais pour lequel CanBeMisguiding vaut true également,
        /// ne sera pas rendu visible par cette méthode.
        /// </remarks>
        private bool ShowAllImportantAndUnambiguousAttributesForEachElement()
        {
            bool success = false;

            foreach(XPathElement element in elements)
            {
                var singularAttributes = from a in element.Attributes where a.IsSingular && a.Visible select a;
                if (singularAttributes.Any())
                    continue;

                var importantAttributes = from a in element.Attributes where a.IsImportant select a;
                foreach (var importantAttribute in importantAttributes)
                {
                    if (element.CanBeMisguiding(importantAttribute) == false)
                    {
                        importantAttribute.Visible = true;
                        success = true;
                    }
                }
            }

            return success;
        }

        
        private void ShowImportantButMisguidingAttributeForTheLastElement()
        {
            for(int index=elements.Count-1; index>=0;index--)
            {
                XPathElement currentElement = elements[index];
                foreach(XPathAttribute a in currentElement.Attributes)
                {
                    if (a.IsImportant)
                    {
                        if (a.Visible == false)
                        {
                            a.Visible = true;
                            return;
                        }
                    }
                }
            }            
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xpathToTarget"></param>
        /// <returns>Retourne un indicateur de performance du résultat</returns>
        internal int Goto(AbstractXPathBuilder xpathToTarget)
        {
            if ((this.IsAbsolute == false) || (xpathToTarget.IsAbsolute == false))
            {
                throw new Exception();
            }

            int score = 0;
            int index = 0;
            XPathElement thisElement = null;
            XPathElement externalElement = null;

            while ((index < elements.Count) && (index < xpathToTarget.elements.Count))
            {
                thisElement = elements[index];
                externalElement = xpathToTarget.elements[index];

                if( thisElement.Equals(externalElement)==false )
                {
                    break;
                }

                index++;
            }

            int elementsToGetBackFrom = elements.Count - index;

            List<XPathElement> result = new List<XPathElement>();
            for(int i=0;i<elementsToGetBackFrom;i++)
            {
                result.Add(new XPathElement.GoBackElement());
                score++;
            }


            for(int i=index; i<xpathToTarget.elements.Count; i++)
            {
                result.Add(xpathToTarget.elements[i]);
                score++;
            }


            IsAbsolute = false;
            elements = result;
            return score;
        }

        public bool Equals(AbstractXPathBuilder other)
        {
            string xpath1 = this.ToString();
            string xpath2 = other.ToString();
            return xpath1.Equals(xpath2);
        }
    }
}
