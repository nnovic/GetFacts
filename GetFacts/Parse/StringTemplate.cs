using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    public class StringTemplate
    {
        public string XPath
        {
            get;set;
        }

        public string Regex
        {
            get;set;
        }

        public static bool CompareTo(StringTemplate st1, StringTemplate st2)
        {
            if (st1 == st2)
                return true;

            if (string.Compare(st1.XPath, st2.XPath) != 0)
                return false;

            if (string.Compare(st1.Regex, st2.Regex) != 0)
                return false;

            return true;
        }

        public string Execute(XPathNavigator nav)
        {
            XPathNavigator node = nav.SelectSingleNode(XPath);
            if (node == null)
                return string.Empty;

            string innerText = node.Value.Trim();
            innerText = HtmlEntity.DeEntitize(innerText);
            //innerText = HttpUtility.HtmlDecode(innerText);

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
