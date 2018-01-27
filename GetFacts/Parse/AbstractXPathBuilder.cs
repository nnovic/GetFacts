using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        protected virtual void Insert(XPathElement element)
        {
            elements.Insert(0, element);
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


        public static AbstractXPathBuilder SynthetizeAndOptimize(ICollection<AbstractXPathBuilder> expressions)
        {
            AbstractXPathBuilder result = Synthetize(expressions);
            result.Optimize();
            return result;
        }

        /// <summary>
        /// Cette méthode analyse les XPath passés en paramètre et tente de
        /// les synthétiser en une seule expression XPath englobante.
        /// </summary>
        /// <param name="expressions">Ensemble des expressions XPath qu'il faut essayer de synthetiser.
        /// La collection doit contenir au moins deux éléments.</param>
        /// <returns>null si aucune synthèse n'a pu être établie, ou si la collection passée en paramètre contient moins de deux éléments.</returns>
        /// <exception cref="ArgumentNullException">"expressions" ne peut pas être null</exception>
        /// <remarks>
        /// Les expressions en argument devraient de préférence être optimisées.
        /// L'expression resultante n'est pas optimisee.
        /// </remarks>
        public static AbstractXPathBuilder Synthetize(ICollection<AbstractXPathBuilder> expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException("expressions");

            if (expressions.Count <= 1)
                return null;

            // trier par ordre décroissant de la taille
            // des expressions XPath :
            IEnumerable <AbstractXPathBuilder> orderedList = expressions.OrderByDescending( xpath => xpath.Elements.Count );
            int expressionsCount = orderedList.Count();
            AbstractXPathBuilder referenceXPathBuilder = orderedList.ElementAt(0);

            // créer un tableau pour stocker
            // les index de recherche de chaque
            // expression XPath:
            int[] indexes = new int[expressionsCount];
            for(int i=0; i< expressionsCount; i++)
            {
                indexes[i] = orderedList.ElementAt(i).Elements.Count-1;
                if( indexes[i]<0 )
                {
                    throw new ArgumentException();
                }
            }

            // initialisation de l'expression synthétique
            AbstractXPathBuilder synthetesis = new NaiveXPathBuilder();
            bool stopSynthetesis = false;
            bool allDone;

            // Boucle perettant de faire varier
            // les index (index qui pointent sur
            // les éléments en cours d'évaluation
            // au sein de chaque expression):
            do
            {
                // Pour chaque expression, comparer entre eux les XPathElements
                // dont les indes sont spécifiés dans "indexes".
                // (le premier AbstractXPathBuilder de "orderedList" sert de référence pour
                // effectuer la comparaison).
                NaiveXPathBuilder.NaiveXPathElement referenceElement = new NaiveXPathBuilder.NaiveXPathElement(referenceXPathBuilder.Elements[indexes[0]]);
                for (int xpIndex = 1; xpIndex < expressionsCount; xpIndex++)
                {
                    AbstractXPathBuilder comparedXPathBuilder = orderedList.ElementAt(xpIndex);
                    XPathElement comparedElement = comparedXPathBuilder.Elements[indexes[xpIndex]];

                    // Faire une simple recherche sur le nom de l'élément:
                    // si les éléments comparés n'ont pas le même nom,
                    // on va vérifier si on peut trouver une correspondance
                    // en avançant un peu dans les index.
                    if (string.Equals(comparedElement.ElementName, referenceElement.ElementName) == false)
                    {
                        int lookupStartIndex = indexes[0] - 1;
                        int lookupMinIndex = indexes[xpIndex];
                        int possibleMatchAt = -1;

                        while( lookupStartIndex>=lookupMinIndex )
                        {
                            XPathElement lookupElement = referenceXPathBuilder.Elements[lookupStartIndex];
                            if (string.Equals(comparedElement.ElementName, lookupElement.ElementName) )
                            {
                                possibleMatchAt = lookupStartIndex;
                                break;
                            }
                            lookupStartIndex--;
                        }

                        if( possibleMatchAt!=-1 )
                        {
                            indexes[xpIndex]++;
                            referenceElement.Dumped = true;
                            continue;
                        }
                    }


                    // comparer l'élement sélectionné avec l'élément par référence.
                    NaiveXPathBuilder.NaiveXPathElement intersection = NaiveXPathBuilder.NaiveXPathElement.Intersection(referenceElement,comparedElement);
                    referenceElement = intersection;
                }


                synthetesis.Insert(referenceElement);

                // Faire régresser les index dans le tableau
                allDone = true;
                for(int xpIndex=0; xpIndex<expressionsCount; xpIndex++)
                {
                    indexes[xpIndex]--;
                    if( indexes[xpIndex]< 0 )
                    {
                        stopSynthetesis = true;
                    }
                    else
                    {
                        allDone = false;
                    }
                }

            }while (!stopSynthetesis) ;

            if(allDone==false)
            {
                throw new Exception("should never happen");
            }


            return synthetesis;
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
                    if (importantAttribute.CanBeMisguiding == false)
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
                if (currentElement.HasAnyVisibleAttribute)
                    return;

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
