using GetFacts.Parse;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFactsTests
{
    [TestClass]
    public class AbstractXPathBuilder_Synthetize_Tests
    {
        private string GenerateHtmlPageForSynthetizeTests()
        {
            StringBuilder rawHtml = new StringBuilder();
            rawHtml.AppendLine("<html>");
            rawHtml.AppendLine("    <head>");
            rawHtml.AppendLine("        <title>TEST</title>");
            rawHtml.AppendLine("    </head>");
            rawHtml.AppendLine("    <body>");
            rawHtml.AppendLine("        <div>");
            rawHtml.AppendLine("            <section>");
            rawHtml.AppendLine("                <article id=\"article1\">");
            rawHtml.AppendLine("                    <h2>article 1</h2>");
            rawHtml.AppendLine("                </article>");
            rawHtml.AppendLine("            </section>");
            rawHtml.AppendLine("        </div>");
            rawHtml.AppendLine("        <div>");
            rawHtml.AppendLine("            <section>");
            rawHtml.AppendLine("                <article id=\"article2\">");
            rawHtml.AppendLine("                    <h2>article 2</h2>");
            rawHtml.AppendLine("                </article>");
            rawHtml.AppendLine("            </section>");
            rawHtml.AppendLine("            <div>");
            rawHtml.AppendLine("                <section>");
            rawHtml.AppendLine("                    <article id=\"article3\">");
            rawHtml.AppendLine("                        <h2>article 3</h2>");
            rawHtml.AppendLine("                    </article>");
            rawHtml.AppendLine("                </section>");
            rawHtml.AppendLine("            </div>");
            rawHtml.AppendLine("        </div>");
            rawHtml.AppendLine("    </body>");
            rawHtml.AppendLine("</html>");
            return rawHtml.ToString();
        }

        /// <summary>
        /// "article1" et "article2" sont dans des hiérarchies différentes, mais
        /// l'expression Xpath qui permet de les trouver est la même, soit
        /// "/html/body/div/section/article[@id]/h2". Cette expression pourra
        /// même être optimisée en "//article[@id]/h2".
        /// Ce test vérifie la capacité de AbstractXPathBuilder.Synthetize()
        /// à trouver cette solution.
        /// </summary>
        [TestMethod]
        public void AbstractXPathBuilder_Synthetize_ExactSamePathsButDifferentHierarchies()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GenerateHtmlPageForSynthetizeTests());

            HtmlNode article1 = doc.DocumentNode.SelectSingleNode("//*[@id=\"article1\"]/h2");
            AbstractXPathBuilder xpath1 = new HtmlXPathBuilder();
            xpath1.Build(article1);

            HtmlNode article2 = doc.DocumentNode.SelectSingleNode("//*[@id=\"article2\"]/h2");
            AbstractXPathBuilder xpath2 = new HtmlXPathBuilder();
            xpath2.Build(article2);

            List<AbstractXPathBuilder> list = new List<AbstractXPathBuilder>();
            list.Add(xpath1); 
            list.Add(xpath2); 
            AbstractXPathBuilder synthetisis = HtmlXPathBuilder.Synthetize(list);
            string expectedResult = "/html/body/div/section/article[@id]/h2";
            string actualResult = synthetisis.ToString();
            Assert.AreEqual(expectedResult, actualResult);

            synthetisis.Optimize();
            string expectedOptimization = "//article[@id]/h2";
            string optimizedResult = synthetisis.ToString();
            Assert.AreEqual(expectedOptimization, optimizedResult);
        }


        /// <summary>
        /// "article1" et "article3" sont dans des hiérarchies différentes, et
        /// les expressions XPath permettant de les trouver sont proches. En fait,
        /// elles ne diffèrent que d'un "div" en plus dans le chemin d' "article3".
        /// /html/body/div/section/article[@id]/h2 --> article1
        /// /html/body/div/div/section/article[@id]/h2 --> article3
        /// La synthèse attendue combine les deux expressions en tenant
        /// compte de cette différence.
        /// </summary>
        [TestMethod]
        public void AbstractXPathBuilder_Synthetize_AlmostSamePathsWithDepthDifference()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GenerateHtmlPageForSynthetizeTests());

            HtmlNode article1 = doc.DocumentNode.SelectSingleNode("//*[@id=\"article1\"]/h2");
            AbstractXPathBuilder xpath1 = new HtmlXPathBuilder();
            xpath1.Build(article1);

            HtmlNode article3 = doc.DocumentNode.SelectSingleNode("//*[@id=\"article3\"]/h2");
            AbstractXPathBuilder xpath3 = new HtmlXPathBuilder();
            xpath3.Build(article3);

            List<AbstractXPathBuilder> list = new List<AbstractXPathBuilder>();
            list.Add(xpath1);
            list.Add(xpath3);
            AbstractXPathBuilder synthetisis = HtmlXPathBuilder.Synthetize(list);
            string expectedResult = "/html/body//div/section/article[@id]/h2";
            string actualResult = synthetisis.ToString();
            Assert.AreEqual(expectedResult, actualResult);

            synthetisis = HtmlXPathBuilder.SynthetizeAndOptimize(list);
            string expectedOptimization = "//article[@id]/h2";
            string optimizedResult = synthetisis.ToString();
            Assert.AreEqual(expectedOptimization, optimizedResult);
        }

    }
}
