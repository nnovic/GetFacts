using GetFacts.Parse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFactsTests
{
    [TestClass]
    public class HtmlParserTests
    {
        public static readonly string[] HtmlSamples = new string[]
        {
           "html_sample"
        };

        /// <summary>
        /// Cette procédure de test va tenter de charger tous les échantillons
        /// HTML et vérifier qu'aucune erreur ne se produise. Pour réaliser
        /// ce test, chaque fichier va être chargé dans la même instance de HtmlParser.
        /// </summary>
        [TestMethod]
        public void LoadAllHtmlSamples()
        {
            HtmlParser parser = new HtmlParser();

            foreach (string sample in HtmlSamples)
            {
                byte[] bytes = (byte[])Samples.ResourceManager.GetObject(sample);
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    parser.Load(stream, null);
                }
            }
        }
    }
}
