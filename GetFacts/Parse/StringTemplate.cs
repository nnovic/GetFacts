﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    public class StringTemplate:IEquatable<StringTemplate>
    {
        [DefaultValue(null)]
        public string XPath
        {
            get;set;
        }

        [DefaultValue(null)]
        public string Regex
        {
            get;set;
        }


        bool IEquatable<StringTemplate>.Equals(StringTemplate other)
        {
            if(other == null)
            {
                return false;
            }

            return (string.Compare(XPath, other.XPath) == 0)
                && (string.Compare(Regex, other.Regex) == 0);
        }

        /// <summary>
        /// Indique si le template sert à quelque chose, ou pas.
        /// </summary>
        [JsonIgnore]
        public bool IsNullOrEmpty
        {
            get
            {
                return string.IsNullOrEmpty(XPath) && string.IsNullOrEmpty(Regex);
            }
        }

        protected virtual string NodesToString(XPathNodeIterator nodes)
        {
            StringBuilder concat = new StringBuilder();
            foreach (XPathNavigator node in nodes)
            {
                concat.Append(node.Value.Trim());
                concat.Append(' ');
            }

            return concat.ToString().TrimEnd();
        }

        /// <summary>
        /// Cette version de Execute fait appel à Execute(XPathNavigator),
        /// et réalise les opérations suivantes sur le résultat:
        /// - supprime toutes les balises HTML du texte
        /// - coupe le texte s'il dépasse maxChars caractères
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="maxChars"></param>
        /// <returns></returns>
        public string Execute(XPathNavigator nav, int maxChars)
        {
            string output = Execute(nav);

            if (string.IsNullOrEmpty(output))
                return output;

            HtmlDocument htmldoc = new HtmlDocument();
            htmldoc.LoadHtml(output);
            output = htmldoc.DocumentNode.InnerText;

            if (output.Length > maxChars)
            {
                output = output.Substring(0, maxChars) + " [...]";
            }
            return output;
        }

        public string Execute(XPathNavigator nav)
        {
            if (string.IsNullOrEmpty(XPath))
                return string.Empty;

            XPathNodeIterator nodes;
            //XPathNavigator node;

            try
            {
                //node = nav.SelectSingleNode(XPath, (IXmlNamespaceResolver)nav);
                nodes = nav.Select(XPath, (IXmlNamespaceResolver)nav);
                if (nodes == null)
                    return string.Empty;
            }
            catch(XPathException)
            {
                return string.Empty;
            }

            string innerText = NodesToString(nodes);
            innerText = HtmlEntity.DeEntitize(innerText);

            if( string.IsNullOrEmpty(Regex) )
            {
                return innerText;
            }
            else
            {
                Regex regex = new Regex(Regex);
                MatchCollection matches = regex.Matches(innerText);
                StringBuilder result = new StringBuilder();
                foreach(Match m in matches)
                {
                    foreach(Capture c in m.Captures)
                    {
                        result.Append(c.Value);
                    }
                }
                return result.ToString();
            }
        }


    }
}
