using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    public class AbstractTemplate
    {
        public StringTemplate IdentifierTemplate
        {
            get; set;
        }

        public StringTemplate TitleTemplate
        {
            get; set;
        }

        public StringTemplate TextTemplate
        {
            get; set;
        }

        public StringTemplate IconUrlTemplate
        {
            get; set;
        }

        public StringTemplate MediaUrlTemplate
        {
            get;
        }

        public StringTemplate BrowserUrlTemplate
        {
            get; set;
        }

        protected virtual bool CompareTo(AbstractTemplate at)
        {
            if (StringTemplate.CompareTo(IdentifierTemplate, at.IdentifierTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(TitleTemplate,at.TitleTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(TextTemplate,at.TextTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(IconUrlTemplate,at.IconUrlTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(MediaUrlTemplate,at.MediaUrlTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(BrowserUrlTemplate,at.BrowserUrlTemplate) == false)
                return false;

            return true;
        }
    }
}
