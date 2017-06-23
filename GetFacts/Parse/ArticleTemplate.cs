using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    public class ArticleTemplate:AbstractTemplate
    {
        public string XPathFilter
        {
            get; set;
        }

        public bool CompareTo(ArticleTemplate st)
        {
            if (string.Compare(XPathFilter, st.XPathFilter) != 0)
                return false;

            if (base.CompareTo(st) == false)
                return false;

            return true;
        }
    }
}
