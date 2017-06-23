using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    public class SectionTemplate:AbstractTemplate
    {
        private readonly List<ArticleTemplate> articles = new List<ArticleTemplate>();

        public string SectionName
        {
            get; set;
        }

        public string XPathFilter
        {
            get; set;
        }

        public IList<ArticleTemplate> Articles
        {
            get { return articles; }
        }

        public bool CompareTo(SectionTemplate st)
        {
            if (string.Compare(SectionName, st.SectionName) != 0)
                return false;

            if (string.Compare(XPathFilter, st.XPathFilter) != 0)
                return false;

            if (base.CompareTo(st) == false)
                return false;

            int count1 = Articles.Count;
            int count2 = st.Articles.Count;
            if (count1 != count2)
                return false;

            for (int index = 0; index < count1; index++)
            {
                ArticleTemplate s1 = Articles[index];
                ArticleTemplate s2 = st.Articles[index];
                if (s1.CompareTo(s2) == false)
                    return false;
            }

            return true;
        }
    }
}
