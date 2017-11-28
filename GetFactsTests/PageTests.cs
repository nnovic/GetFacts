using GetFacts.Facts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFactsTests
{
    [TestClass]
    public class PageTests
    {
        const string GenericSectionName = "section";

        const string TitleForSection1 = "Title of section 1";
        const string TextForSection1 = "This text describes the first section.";
        const int IndexForSection1 = 0;

        const string TitleForSection2 = "Title of section 2";
        const string TextForSection2 = "This text describes the second section.";
        const int IndexForSection2 = 1;

        const string TitleForSection3 = "Title of section 3";
        const string TextForSection3 = "This text describes the third section.";
        const int IndexForSection3 = 2;

        Page CreateTestStructure1()
        {
            Page page = new Page("https://nnovic.github.io/GetFacts/");

            Section section1 = page.AddSection(GenericSectionName, TitleForSection1, TextForSection1);
            Section section2 = page.AddSection(GenericSectionName, TitleForSection2, TextForSection2);

            return page;
        }


        /// <summary>
        /// A la création de la page, la propriété Page.PageName
        /// est initialisé avec la valeur de PageConfig.Name
        /// Cependant, si PageConfig.Name est null ou vide, alors
        /// Page.PageName prend la valeur de Template.PageName
        /// </summary>
        public void TestPageName()
        {

        }

        /// <summary>
        /// L'objectif de cette méthod est de tester le bon
        /// fonctionnement de Page.FindSection(string,string,string).
        /// 
        /// 
        /// </summary>
        [TestMethod]
        public void TestPageFindSection()
        {
            Page page = CreateTestStructure1();

            // Vérifier qu'on obtient la section par défaut
            // systématiquement si le paramètre "name" est null ou vide.

            Assert.ReferenceEquals(page.DefaultSection, page.FindSection(null, TitleForSection1, null));
            Assert.ReferenceEquals(page.DefaultSection, page.FindSection(null, null, TextForSection1));

            Assert.ReferenceEquals(page.DefaultSection, page.FindSection(null, TitleForSection2, null));
            Assert.ReferenceEquals(page.DefaultSection, page.FindSection(null, null, TextForSection2));

            Assert.ReferenceEquals(page.DefaultSection, page.FindSection(null, null, null));

            // Vérifier, dans le cas nominal, qu'on retrouve bien
            // les sections qui sont définies dans la structure de test

            Section section1 = page.GetSection(IndexForSection1);
            Assert.ReferenceEquals(section1, page.FindSection(GenericSectionName, TitleForSection1, TextForSection1));
            Assert.ReferenceEquals(section1, page.FindSection(GenericSectionName, TitleForSection1, null));
            Assert.ReferenceEquals(section1, page.FindSection(GenericSectionName, null, TextForSection2));

            Section section2 = page.GetSection(IndexForSection2);
            Assert.ReferenceEquals(section2, page.FindSection(GenericSectionName, TitleForSection2, TextForSection2));
            Assert.ReferenceEquals(section2, page.FindSection(GenericSectionName, TitleForSection2, null));
            Assert.ReferenceEquals(section2, page.FindSection(GenericSectionName, null, TextForSection2));

            Assert.AreNotSame(section1, section2);

            // Vérifier, dans le cas d'une section dont le contenu
            // est légèrement altéré, que la recherche donne le résultat
            // attendu

            Assert.ReferenceEquals(section1, page.FindSection(GenericSectionName, TitleForSection3, TextForSection1));
            Assert.ReferenceEquals(section1, page.FindSection(GenericSectionName, TitleForSection1, TextForSection3));

            Assert.ReferenceEquals(section2, page.FindSection(GenericSectionName, TitleForSection3, TextForSection2));
            Assert.ReferenceEquals(section2, page.FindSection(GenericSectionName, TitleForSection2, TextForSection3));

            // Vérifier que, en cas de recherche d'une section inexistante,
            // une nouvelle section est créée:

            Section section3 = page.FindSection(GenericSectionName, TitleForSection3, TextForSection3);
            Assert.IsNotNull(section3);
            Assert.AreNotSame(section1, section3);
            Assert.AreNotSame(section2, section3);

            // Recherche ambigüe: le titre correspond à la section1, mais le texte
            // correspond à la section2.
            // Dans ce cas de figure, le résultat attendu est la création d'une toute nouvelle section.

            Section section4 = page.FindSection(GenericSectionName, TitleForSection1, TextForSection2);
            Assert.IsNotNull(section4);
            Assert.AreNotSame(section1, section4);
            Assert.AreNotSame(section2, section4);
            Assert.AreNotSame(section3, section4);
            Assert.AreSame(section4, page.FindSection(GenericSectionName, TitleForSection1, TextForSection2)); // vérifie la répétabilité

            // Recherche ambigüe: le titre et le texte ne sont pas précisés,
            // alors qu'il existe plusieurs sections portant le même nom:
            // Dans ce cas de figure, le résultat attendu est la création d'une toute nouvelle section.

            Section section5 = page.FindSection(GenericSectionName, null, null);
            Assert.IsNotNull(section5);
            Assert.AreNotSame(section1, section5);
            Assert.AreNotSame(section2, section5);
            Assert.AreNotSame(section3, section5);
            Assert.AreNotSame(section4, section5);
            Assert.AreSame(section5, page.FindSection(GenericSectionName, null, null)); // vérifie la répétabilité
        }


        /// <summary>
        /// Après un Update, la propriété  Title de la Page
        /// doit correspondre au contenu du code source qui a été analysé.
        /// Cependant, si l'analyse n'a pas pu trouver de titre, alors la
        /// propritété Page.Title est mise par défaut à Page.PageName
        /// </summary>
        public void TestPageTitle()
        {

        }


        public void TestDefaultSection()
        {

        }
    }
}
