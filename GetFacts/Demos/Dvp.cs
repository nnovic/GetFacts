using GetFacts.Facts;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Demos
{
    public class Dvp:Page
    {
        public Dvp():base("https://www.developpez.com/")
        {
            Parser = new HtmlParser();
            Template = TemplateFactory.GetInstance().GetTemplate(@"fr\www.developpez.com.json");
        }
    }
}
