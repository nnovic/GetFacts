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
    public class XmlParserTests
    {
        public static readonly string[] XmlSamples = new string[] 
        {
           "rss_sample"
        };

        /// <summary>
        /// Cette procédure de test va tenter de charger tous les échantillons
        /// XML et vérifier qu'aucune erreur ne se produise. Pour réaliser
        /// ce test, chaque fichier va être chargé dans la même instance de XmlParser.
        /// </summary>
        [TestMethod]
        public void LoadAllXmlSamples()
        {
            XmlParser parser = new XmlParser();

            foreach (string sample in XmlSamples)
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
