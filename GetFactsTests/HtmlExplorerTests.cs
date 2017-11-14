using GetFacts.Parse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GetFactsTests
{
    [TestClass]
    public class HtmlExplorerTests
    {
        /// <summary>
        /// L'objectif est de vérifier que, avec les données telles qu'elles 
        /// s'afficheraient dans TemplatesApp, la passerelle FlowDocument 
        /// vers Tree est 100% fonctionelle. Autrement dit, il s'agit de vérifier
        /// que si on clique sur n'importe quel Hyperlink du FlowDocument
        /// on obtiendra son pendant dans le Tree.
        /// </summary>
        [TestMethod]
        public void ClickOnAllHyperlinks()
        {
            foreach (string sample in HtmlParserTests.HtmlSamples)
            {
                byte[] bytes = (byte[])Samples.ResourceManager.GetObject(sample);
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    HtmlParser parser = new HtmlParser();
                    parser.Load(stream, null);

                    FlowDocument doc = parser.SourceCode;
                    Assert.IsNotNull(parser.SourceTree); // Force la création de l'arborescence, sans quoi le test va échouer.

                    uint hits = FlowDocumentWalker.Walk(doc, te =>
                    {
                        if (te is Hyperlink hl)
                        {
                            TreeViewItem tvi = parser.TextElementToTreeViewItem(hl);
                            Assert.IsNotNull(tvi);
                            return true;
                        }
                        return false;
                    });
                    Assert.IsFalse(hits == 0);
                }
            }
        }
    }
}
