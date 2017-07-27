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
    }
}
