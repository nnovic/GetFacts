using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Facts
{
    public class PageConfig
    {
        public PageConfig()
        {
            Refresh = 60;
        }

        public string Name
        {
            get; set;
        }

        public string Url
        {
            get; set;
        }

        public string Template
        {
            get; set;
        }

        /// <summary>
        /// (en minutes)
        /// </summary>
        public int Refresh
        {
            get; set;
        }
    }
}
