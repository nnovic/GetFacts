using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    /// <summary>
    /// Cette implémentation de AbstractXPathBuilder
    /// est destinée à la méthode AbstractXPathBuilder.Synthetize, afin
    /// de générer une instance d'AbstractXPathBuilder sans avoir besoin
    /// de connaître l'implémentation réelle (Html, Xml, ...).
    /// </summary>
    internal class NaiveXPathBuilder : AbstractXPathBuilder
    {
        protected override void BuildImpl(object target)
        {
        }

        
        // 
        protected override void Insert(XPathElement element)
        {
            if (Elements == null)
            {
                Build(null);
            }
            base.Insert(element);
        }

        /// <summary>
        /// Cette implémentation de XPathElement est destinée à
        /// être utilisée par AbstractXPathBuilder.Synthetize durant la création
        /// d'une instance de NaiveXPathBuilder.
        /// Sa principale caractéristique est d'être totalement
        /// indépendante de l'implémentation réelle (Html, Xml, ...).
        /// </summary>
        internal class NaiveXPathElement : XPathElement
        {
            private readonly string elementName;

            /// <summary>
            /// Constructeur par recopie.
            /// </summary>
            /// <param name="src"></param>
            public NaiveXPathElement(XPathElement src)
            {
                elementName = src.ElementName;
                foreach (XPathAttribute a in src.Attributes)
                {
                    Attributes.Add(new NaiveXPathAttribute(a));
                }
            }

            public NaiveXPathElement(string elementName)
            {
                this.elementName = elementName;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="element"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">'e1' ou 'e2' est null, ce qui n'est pas supporté</exception>
            /// <exception cref="ArgumentException">'e1' ou 'e2' est de type GoBackElement ou WildCardElemnt, ce qui n'est pas supporté</exception>
            public static NaiveXPathElement Intersection(XPathElement e1, XPathElement e2)
            {
                if (e1 == null) throw new ArgumentNullException("e1");
                if (e2 == null) throw new ArgumentNullException("e2");

                if ((e1 is GoBackElement) || (e1 is WildCardElement)) throw new ArgumentException();
                if ((e2 is GoBackElement) || (e2 is WildCardElement)) throw new ArgumentException();

                // Si les deux XPathElement passés en argument ont
                // exactement le même nom, alors le résultat
                // de l'intersection va porter ce nom. Sinon, on
                // va retourne un élément '*' (wildcard).
                NaiveXPathElement output = null;
                if( string.Equals(e1.ElementName, e2.ElementName) )
                {
                    output = new NaiveXPathElement(e1.ElementName);
                }
                else
                {
                    output = new WildCardElement();
                }

                // à valider:
                output.Dumped = e1.Dumped | e2.Dumped;

                // Rechercher tous les attributs qui portent
                // le même nom dans 'e1' et 'e2'.
                foreach (XPathAttribute referenceAttribute in e1.Attributes)
                {
                    // On ne prend pas en compte les attributs
                    // qui apparaissent en plusieurs exemplaires
                    // avec le même nom:
                    var sameNameAttributes = from attribute in e1.Attributes
                                             where attribute.Name == referenceAttribute.Name
                                             select attribute;

                    var matchingAttributes = from attribute in e2.Attributes
                                             where attribute.Name==referenceAttribute.Name
                                             select attribute;

                    // On recherche une correspondance
                    // et une seule.
                    if((sameNameAttributes.Count()==1)&&(matchingAttributes.Count()==1))
                    {
                        XPathAttribute matchingAttribute = matchingAttributes.First();
                        NaiveXPathAttribute outputAttribute = new NaiveXPathAttribute(matchingAttribute);

                        outputAttribute.isSingular = referenceAttribute.IsSingular | matchingAttribute.IsSingular;
                        outputAttribute.isImportant = referenceAttribute.IsImportant | matchingAttribute.IsImportant;

                        // Maintenant, en plus du nom, on regarde
                        // si les attributs ont la même valeur
                        if (string.Compare(referenceAttribute.Value, matchingAttribute.Value)==0)
                        {
                            outputAttribute.Value = referenceAttribute.Value;
                            outputAttribute.canBeMisguiding = referenceAttribute.CanBeMisguiding | matchingAttribute.CanBeMisguiding;
                        }
                        else
                        {
                            outputAttribute.Value = null;
                            outputAttribute.canBeMisguiding = false;
                        }

                        output.Attributes.Add(outputAttribute);
                    }
                }

                return output;
            }

            
            public override string ElementName => elementName;
            
            protected override object ConcreteElement => throw new NotImplementedException();

            public override bool Equals(XPathElement other)
            {
                return base.Equals((object)other);
            }
        }

        /// <summary>
        /// Cette implémentation de XPathAttribute est destinée à
        /// être utilisée par AbstractXPathBuilder.Synthetize durant la création
        /// d'une instance de NaiveXPathBuilder.
        /// Sa principale caractéristique est d'être totalement
        /// indépendante de l'implémentation réelle (Html, Xml, ...).
        /// </summary>
        internal class NaiveXPathAttribute:XPathAttribute
        {
            internal bool isSingular=false;
            internal bool isImportant=false;
            internal bool canBeMisguiding=false;

            public NaiveXPathAttribute(XPathAttribute src):base(src.Name, src.Value)
            {
                isSingular = src.IsSingular;
                isImportant = src.IsImportant;
                canBeMisguiding = src.CanBeMisguiding;
                //Visible = true;
            }

            internal NaiveXPathAttribute(string name, string value) : base(name, value)
            {
                //Visible = true;
            }

            public override bool IsSingular => isSingular;

            public override bool IsImportant => isImportant;

            public override bool CanBeMisguiding => canBeMisguiding;
        }

        
        internal class WildCardElement : NaiveXPathElement
        {
            public WildCardElement():base("*")
            {
            }

            public override bool Equals(XPathElement other)
            {
                return other is WildCardElement;
            }
        }
    }
}
