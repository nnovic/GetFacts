using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFactsTests.Facts
{
    /// <summary>
    /// L'objectif de cette classe de test est
    /// de vérifier que les objets "Testable..." 
    /// fonctionnent comme attendu.
    /// </summary>
    [TestClass()]
    public class SelfTest
    {
        [TestMethod()]
        public void CheckRotationTestConfig()
        {
            TestableFacts facts = new TestableFacts("RotationTestConfig.json");

            // Vérifie qu'un appel à Facts.GetInstance() va bien retourner
            // l'instance de TestableFacts qu'on vient de créer.
            Assert.ReferenceEquals(facts, GetFacts.Facts.Facts.GetInstance());

            List<GetFacts.Facts.Page> pages = facts.GetPages();
            Assert.AreEqual(pages.Count, 2);
            Assert.AreEqual<string>(pages[0].Url, "http://www.site1.com/index.html");
            Assert.AreEqual<string>(pages[1].Url, "http://www.site2.com/index.html");
        }
    }
}
