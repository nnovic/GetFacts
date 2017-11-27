using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GetFacts.Parse;
using System.IO;
using HtmlAgilityPack;
using System.Text;

namespace GetFactsTests
{
    [TestClass]
    public class HtmlXPathBuilderTests
    {
        private HtmlDocument Load(string resourceName)
        {
            HtmlDocument doc = new HtmlDocument();
            byte[] bytes = (byte[])Samples.ResourceManager.GetObject(resourceName);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                doc.Load(stream);
            }
            return doc;
        }

        [TestMethod]
        public void TestUltraBasique()
        {
            const string html_sample_name = "html_sample";
            const string element_with_id = "pixel";
            const string raw_xpath = "/html[1]/body[1]/div[1]";

            HtmlDocument doc = Load(html_sample_name);

            HtmlNode nodeWithId = doc.GetElementbyId(element_with_id);
            Assert.AreEqual<string>(nodeWithId.XPath, raw_xpath);

            AbstractXPathBuilder builder = new HtmlXPathBuilder();
            builder.Build(nodeWithId);

            XPathElement htmlElement = builder.Elements[0];

            XPathElement bodyElement = builder.Elements[1];
            XPathAttribute classAttribute = bodyElement.Attributes.Find("class");
            Assert.IsNotNull(classAttribute);
            Assert.IsFalse(classAttribute.IsSingular);
            Assert.IsTrue(classAttribute.IsImportant);

            XPathElement divElement = builder.Elements[2];
            XPathAttribute idAttribute = divElement.Attributes.Find("id");
            Assert.IsNotNull(idAttribute);
            Assert.IsTrue(idAttribute.IsSingular);
        }


        private string GenerateHtmlPageForGotoTests()
        {
            StringBuilder rawHtml = new StringBuilder();
            rawHtml.AppendLine("<html>");
            rawHtml.AppendLine("<head>");
            rawHtml.AppendLine("<title>TEST</title>");
            rawHtml.AppendLine("</head>");
            rawHtml.AppendLine("<body>");
            rawHtml.AppendLine("<div>");
            rawHtml.AppendLine("<h1>LIST1:</h1>");
            rawHtml.AppendLine("<ul>");
            rawHtml.AppendLine("<li>ITEM1</li>");
            rawHtml.AppendLine("<li>ITEM2</li>");
            rawHtml.AppendLine("<li>ITEM3</li>");
            rawHtml.AppendLine("</ul>");
            rawHtml.AppendLine("</div>");
            rawHtml.AppendLine("<div>");
            rawHtml.AppendLine("<h1>LIST2:</h1>");
            rawHtml.AppendLine("<ul>");
            rawHtml.AppendLine("<li>ITEMA</li>");
            rawHtml.AppendLine("<li>ITEMB</li>");
            rawHtml.AppendLine("<li>ITEMC</li>");
            rawHtml.AppendLine("</ul>");
            rawHtml.AppendLine("</div>");
            rawHtml.AppendLine("</body>");
            rawHtml.AppendLine("</html>");
            return rawHtml.ToString();
        }

        /// <summary>
        /// Verifie le fonctionnement de la méthode AbstractXPathBuilder.Goto
        /// en se basant sur du HTML.
        /// </summary>
        /// <see cref="AbstractXPathBuilder.Goto(AbstractXPathBuilder)"/>
        [TestMethod]
        public void TestGotoTowardChild()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GenerateHtmlPageForGotoTests());

            HtmlNode firstDivNode = doc.DocumentNode.SelectSingleNode("//div");
            AbstractXPathBuilder divBuilder = new HtmlXPathBuilder();
            divBuilder.Build(firstDivNode);
            Assert.AreEqual<string>("/html/body/div", divBuilder.ToString());

            HtmlNode firstH1Node = doc.DocumentNode.SelectSingleNode("//h1");
            AbstractXPathBuilder h1Builder = new HtmlXPathBuilder();
            h1Builder.Build(firstH1Node);
            Assert.AreEqual<string>("/html/body/div/h1", h1Builder.ToString());

            int score = divBuilder.Goto(h1Builder);
            Assert.AreEqual<string>("./h1", divBuilder.ToString());
            Assert.AreEqual(1, score);
        }

        /// <summary>
        /// Verifie le fonctionnement de la méthode AbstractXPathBuilder.Goto
        /// en se basant sur du HTML.
        /// </summary>
        /// <see cref="AbstractXPathBuilder.Goto(AbstractXPathBuilder)"/>
        [TestMethod]
        public void TestGotoTowardGrandChild()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GenerateHtmlPageForGotoTests());

            HtmlNode firstDivNode = doc.DocumentNode.SelectSingleNode("//div");
            AbstractXPathBuilder divBuilder = new HtmlXPathBuilder();
            divBuilder.Build(firstDivNode);
            Assert.AreEqual<string>("/html/body/div", divBuilder.ToString());

            HtmlNode firstH1Node = doc.DocumentNode.SelectSingleNode("//li");
            AbstractXPathBuilder h1Builder = new HtmlXPathBuilder();
            h1Builder.Build(firstH1Node);
            Assert.AreEqual<string>("/html/body/div/ul/li", h1Builder.ToString());

            int score = divBuilder.Goto(h1Builder);
            Assert.AreEqual<string>("./ul/li", divBuilder.ToString());
            Assert.AreEqual(2, score);
        }

        /// <summary>
        /// Verifie le fonctionnement de la méthode AbstractXPathBuilder.Goto
        /// en se basant sur du HTML.
        /// </summary>
        /// <see cref="AbstractXPathBuilder.Goto(AbstractXPathBuilder)"/>
        [TestMethod]
        public void TestGotoTowardParent()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GenerateHtmlPageForGotoTests());

            HtmlNode firstDivNode = doc.DocumentNode.SelectSingleNode("//div");
            AbstractXPathBuilder divBuilder = new HtmlXPathBuilder();
            divBuilder.Build(firstDivNode);
            Assert.AreEqual<string>("/html/body/div", divBuilder.ToString());

            HtmlNode firstH1Node = doc.DocumentNode.SelectSingleNode("//body");
            AbstractXPathBuilder h1Builder = new HtmlXPathBuilder();
            h1Builder.Build(firstH1Node);
            Assert.AreEqual<string>("/html/body", h1Builder.ToString());

            int score = divBuilder.Goto(h1Builder);
            Assert.AreEqual<string>("./..", divBuilder.ToString());
            Assert.AreEqual(1, score);
        }

        /// <summary>
        /// Verifie le fonctionnement de la méthode AbstractXPathBuilder.Goto
        /// en se basant sur du HTML.
        /// </summary>
        /// <see cref="AbstractXPathBuilder.Goto(AbstractXPathBuilder)"/>
        [TestMethod]
        public void TestGotoTowardSelf()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GenerateHtmlPageForGotoTests());

            HtmlNode firstDivNode = doc.DocumentNode.SelectSingleNode("//div");
            AbstractXPathBuilder divBuilder = new HtmlXPathBuilder();
            divBuilder.Build(firstDivNode);
            Assert.AreEqual<string>("/html/body/div", divBuilder.ToString());

            HtmlNode firstH1Node = doc.DocumentNode.SelectSingleNode("//div");
            AbstractXPathBuilder h1Builder = new HtmlXPathBuilder();
            h1Builder.Build(firstH1Node);
            Assert.AreEqual<string>("/html/body/div", h1Builder.ToString());

            int score = divBuilder.Goto(h1Builder);
            Assert.AreEqual<string>(".", divBuilder.ToString());
            Assert.AreEqual(0, score);
        }

        /// <summary>
        /// Verifie le fonctionnement de la méthode AbstractXPathBuilder.Goto
        /// en se basant sur du HTML.
        /// </summary>
        /// <see cref="AbstractXPathBuilder.Goto(AbstractXPathBuilder)"/>
        [TestMethod]
        public void TestGotoTowardChildInAnotherHierarchy()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GenerateHtmlPageForGotoTests());

            HtmlNode firstDivNode = doc.DocumentNode.SelectSingleNode("//div");
            AbstractXPathBuilder divBuilder = new HtmlXPathBuilder();
            divBuilder.Build(firstDivNode);
            Assert.AreEqual<string>("/html/body/div", divBuilder.ToString());

            HtmlNode firstH1Node = doc.DocumentNode.SelectNodes("//li").Last();
            AbstractXPathBuilder h1Builder = new HtmlXPathBuilder();
            h1Builder.Build(firstH1Node);
            Assert.AreEqual<string>("/html/body/div/ul/li", h1Builder.ToString());

            int score = divBuilder.Goto(h1Builder);
            Assert.AreEqual<string>("./ul/li", divBuilder.ToString());
            Assert.AreEqual(2, score);
        }
    }
}
