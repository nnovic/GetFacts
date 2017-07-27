using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    public class SectionTemplate:AbstractTemplate
    {
        private readonly ObservableCollection<ArticleTemplate> articles = new ObservableCollection<ArticleTemplate>();

        public string SectionName
        {
            get; set;
        }

        public string XPathFilter
        {
            get; set;
        }


        [JsonProperty(Order = 1000)]
        public ObservableCollection<ArticleTemplate> Articles
        {
            get { return articles; }
        }
        
    }
}
