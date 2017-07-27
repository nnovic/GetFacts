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

        /// <summary>
        /// Compare cette instance de SectionTemplate à
        /// une autre instance, passée en argument.
        /// Deux instances sont équivalentes si:
        /// - Leurs propriétés "SectionName" sont des strings identiques
        /// - Leurs propriétés "XPathFilter" sont des strings identiques
        /// - Leurs propriétés "IdentifierTemplate", "TitleTemplate", 
        /// "TextTemplate", etc... sont identiques
        /// - Les articles contenus sont identiques (en nombre
        /// et en contenu)
        /// </summary>
        /// <param name="st"></param>
        /// <returns>true si les deux instances de SectionTemplate ont
        /// des contenus équivalents.</returns>
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
