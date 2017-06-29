using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFactsTests.Facts
{
    /// <summary>
    /// Surcharge la class GetFacts.Facts.Facts
    /// afin d'exposer les données normalement privées,
    /// et de pouvoir choisir le fichier de configuration.
    /// </summary>
    public class TestableFacts : GetFacts.Facts.Facts
    {
        public TestableFacts(string testConfig) : base(testConfig)
        {
            uniqueInstance = this;
        }

        public List<GetFacts.Facts.Page> GetPages()
        {
            return pages;
        }
    }
}
