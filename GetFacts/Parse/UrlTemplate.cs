using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    public class UrlTemplate:StringTemplate
    {
        protected override string NodesToString(XPathNodeIterator nodes)
        {
            nodes.MoveNext();
            XPathNavigator node = nodes.Current;
            return node.Value.Trim();
        }
    }
}
