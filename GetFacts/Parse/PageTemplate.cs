using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Parse
{
    public class PageTemplate:AbstractTemplate
    {
        private readonly List<SectionTemplate> sections = new List<SectionTemplate>();
        
        public string PageName
        {
            get;set;
        }

        

        public IList<SectionTemplate> Sections
        {
            get { return sections; }
        }


        #region "comparaison"

        public static bool Compare(PageTemplate template1, PageTemplate template2)
        {
            return template1.CompareTo(template2);
        }

        public bool CompareTo(PageTemplate pt)
        {
            if (string.Compare(PageName, pt.PageName) != 0)
                return false;

            if (base.CompareTo(pt) == false)
                return false;

            int count1 = Sections.Count;
            int count2 = pt.Sections.Count;
            if (count1 != count2)
                return false;

            for(int index=0;index<count1;index++)
            {
                SectionTemplate s1 = Sections[index];
                SectionTemplate s2 = pt.Sections[index];
                if (s1.CompareTo(s2) == false)
                    return false;
            }

            return true;
        }

        #endregion
    }
}
