﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Cette classe implémente IEquatable, ce qui signifie
    /// qu'on peut utiliser la méthode .Equals() pour vérifier 
    /// l'égalité de deux XPathElement.</remarks>
    [DebuggerDisplay("XPathElement = {ElementName}")]
    public abstract class XPathElement : IEquatable<XPathElement>
    {
        internal readonly List<XPathAttribute> Attributes = new List<XPathAttribute>();

        public abstract string ElementName { get; }

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

        /// <summary>
        /// Cette propriété permet de masquer ou
        /// montrer cet élément au sein de l'expression XPath;
        /// C'est la fonction d'optimisation qui fera ce choix.
        /// </summary>
        /// <see cref="AbstractXPathBuilder.Optimize"/>
        public bool Visible
        {
            get { return visible && (!Dumped); }
            internal set { visible = value; }
        }

        private bool dumped = false;

        /// <summary>
        /// La fonction de synthèse peut décider que
        /// cet élément doit être éliminé de l'expression
        /// XPath
        /// </summary>
        /// <see cref="AbstractXPathBuilder.Synthetize(ICollection{AbstractXPathBuilder})"/>
        public bool Dumped
        {
            get { return dumped; }
            internal set { dumped = value; }
        }


        public virtual bool Equals(XPathElement other)
        {
            return ConcreteElement.Equals(other.ConcreteElement);
        }
        

        public class GoBackElement : XPathElement
        {
            public override string ElementName => "..";

            protected override object ConcreteElement => null;

            public override bool Equals(XPathElement other)
            {
                return other is GoBackElement;
            }
        }
    }
}
