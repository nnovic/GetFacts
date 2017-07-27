using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace GetFacts.Parse
{
    public class PageTemplate:AbstractTemplate
    {
        private readonly ObservableCollection<SectionTemplate> sections = new ObservableCollection<SectionTemplate>();
        
        /// <summary>
        /// Initialise un PageTemplate avec les valeurs par défaut:
        /// PageType = "HtmlParser"
        /// Charset = "utf-8"
        /// </summary>
        public PageTemplate()
        {
            PageType = AbstractParser.DefaultParser;
            Charset = UTF8Encoding.UTF8.WebName;
        }


        public string PageName
        {
            get;set;
        }

        /// <summary>
        /// définit le parser à utiliser.
        /// </summary>
        [DefaultValue("HtmlParser")]
        public string PageType
        {
            get;
            set;
        }

        /// <summary>
        /// Permet de faire un forçage du Charset (Encoding)
        /// qu'il faudra utiliser pour lire le contenu
        /// brut de la page. Vaut null par défaut, indiquant
        /// que le charset devra être déterminé automatiquement.
        /// </summary>
        [DefaultValue("utf-8")]
        public string Charset
        {
            get;set;
        }

        /// <summary>
        /// Une URL qui est suggérée pour effectuer des tests du template.
        /// </summary>
        public string Reference
        {
            get;set;
        }

        /// <summary>
        /// Retourne null si Charset est null. Sinon,
        /// convertit la chaine Charset en une instance
        /// correspondante de System.Text.Encoding.
        /// Si la conversion Charset > Encoding échoue,
        /// null est retourné.
        /// </summary>
        [JsonIgnore]
        public Encoding Encoding
        {
            get
            {
                if (string.IsNullOrEmpty(Charset))
                    return null;

                try
                {
                    return Encoding.GetEncoding(Charset);
                }
                catch
                {
                    return null;
                }
            }
        }

        [JsonProperty(Order =1000)]
        public ObservableCollection<SectionTemplate> Sections
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
