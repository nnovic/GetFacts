using GetFacts.Facts;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Demos
{
    public class LbcPage:Page
    {
        public LbcPage(string url) :base(url)
        {
            Parser = new HtmlParser();
            Template = TemplateFactory.GetInstance().GetTemplate(@"fr\www.leboncoin.fr.json");
        }
    }
}
