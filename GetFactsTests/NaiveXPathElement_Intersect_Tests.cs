using GetFacts.Parse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFactsTests
{
    [TestClass]
    public class NaiveXPathElement_Intersect_Tests
    {
        [TestMethod]
        public void NaiveXPathElement_Intersect_InvalidParameters()
        {
            // Doit lancer un ArgumentNullException si
            // l'argument 'e1' est null.
            Assert.ThrowsException<ArgumentNullException>(() => 
            {
                XPathElement e1 = null;
                XPathElement e2 = new NaiveXPathBuilder.NaiveXPathElement("dummy");
                NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e2);
            });

            // Doit lancer un ArgumentNullException si
            // l'argument 'e2' est null.
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                XPathElement e1 = new NaiveXPathBuilder.NaiveXPathElement("dummy");
                XPathElement e2 = null;
                NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e2);
            });

            // Doit lancer un ArgumentException si
            // l'argument 'e1' est de type GoBackElement.
            Assert.ThrowsException<ArgumentException>(() =>
            {
                XPathElement e1 = new XPathElement.GoBackElement();
                XPathElement e2 = new NaiveXPathBuilder.NaiveXPathElement("dummy");
                NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e2);
            });

            // Doit lancer un ArgumentException si
            // l'argument 'e1' est de type WildCardElement.
            Assert.ThrowsException<ArgumentException>(() =>
            {
                XPathElement e1 = new NaiveXPathBuilder.WildCardElement();
                XPathElement e2 = new NaiveXPathBuilder.NaiveXPathElement("dummy");
                NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e2);
            });

            // Doit lancer un ArgumentException si
            // l'argument 'e2' est de type GoBackElement.
            Assert.ThrowsException<ArgumentException>(() =>
            {
                XPathElement e1 = new NaiveXPathBuilder.NaiveXPathElement("dummy");
                XPathElement e2 = new XPathElement.GoBackElement();
                NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e2);
            });

            // Doit lancer un ArgumentException si
            // l'argument 'e2' est de type WildCardElement.
            Assert.ThrowsException<ArgumentException>(() =>
            {
                XPathElement e1 = new NaiveXPathBuilder.NaiveXPathElement("dummy");
                XPathElement e2 = new NaiveXPathBuilder.WildCardElement();
                NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e2);
            });
        }

        [TestMethod]
        public void NaiveXPathElement_Intersect_NameComparison()
        {
            // Si les deux XPathElement passés en paramètre
            // ont strictement le même nom, alors le résultat
            // doit porter ce nom.
            NaiveXPathBuilder.NaiveXPathElement e1 = new NaiveXPathBuilder.NaiveXPathElement("name");
            NaiveXPathBuilder.NaiveXPathElement e2 = new NaiveXPathBuilder.NaiveXPathElement("name");
            XPathElement resultWithSameName = NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e2);
            Assert.IsNotNull(resultWithSameName);
            Assert.IsInstanceOfType(resultWithSameName, typeof(NaiveXPathBuilder.NaiveXPathElement));
            Assert.AreEqual(e1.ElementName, e2.ElementName);
            Assert.AreEqual(e1.ElementName, resultWithSameName.ElementName);

            // Si les deux XPathElement passés en paramètre
            // ont des nom différents, alors le résultat est
            // un élément "wildcard" (symbolisé par le "*").
            // Note: une expression XPath est sensible à la casse
            // (minuscule/majuscule).
            NaiveXPathBuilder.NaiveXPathElement e3 = new NaiveXPathBuilder.NaiveXPathElement("NAME");
            XPathElement resultWithDifferentNames = NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e3);
            Assert.IsNotNull(resultWithDifferentNames);
            Assert.IsInstanceOfType(resultWithDifferentNames, typeof(NaiveXPathBuilder.WildCardElement));
            Assert.AreNotEqual(e1.ElementName, e3.ElementName);
            Assert.AreNotEqual(e1.ElementName, resultWithDifferentNames.ElementName);
            Assert.AreEqual("*", resultWithDifferentNames.ElementName);
        }

        [TestMethod]
        public void NaiveXPathElement_Intersect_AttributeComparison()
        {
            NaiveXPathBuilder.NaiveXPathElement e1 = new NaiveXPathBuilder.NaiveXPathElement("name");
            NaiveXPathBuilder.NaiveXPathElement e2 = new NaiveXPathBuilder.NaiveXPathElement("name");

            // Si deux éléments ont des attributs avec le
            // même nom et la même valeur, on doit les
            // retouver dans l'intersection
            e1.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr1", "value"));
            e2.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr1", "value"));

            // Si deux éléments ont des attributs avec le
            // même nom, mais des valeurs différentes, on 
            // doit retrouver un attribut avec le même nom
            // (mais sans valeur définie) dans la solution
            e1.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr2", "value1"));
            e2.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr2", "value2"));

            // Un attribut de e1 qui n'existe pas dans e2,
            // (et inversement), ne se retrouve
            // pas dans la solution.
            e1.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr3", "value"));
            e2.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr4", "value"));

            // La solution ne tient pas compte
            // des attributs qui existeraient
            // en plusieurs exemplaires dans l'un
            // ou l'autre des éléments. D'ailleurs,
            // ce cas de figure ne devrait même pas
            // se présenter !
            e1.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr5", "value1"));
            e1.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr5", "value2"));
            e2.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr5", "value3"));

            e1.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr6", "value1"));
            e2.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr6", "value2"));
            e2.Attributes.Add(new NaiveXPathBuilder.NaiveXPathAttribute("attr6", "value3"));

            // Résultats:

            XPathElement output = NaiveXPathBuilder.NaiveXPathElement.Intersection(e1, e2);
            Assert.IsNotNull(output);
            Assert.IsInstanceOfType(output, typeof(NaiveXPathBuilder.NaiveXPathElement));

            XPathAttribute attr1 = output.Attributes.Find("attr1");
            Assert.IsNotNull(attr1);
            Assert.AreEqual("attr1", attr1.Name);
            Assert.AreEqual("value", attr1.Value);

            XPathAttribute attr2 = output.Attributes.Find("attr2");
            Assert.IsNotNull(attr2);
            Assert.AreEqual("attr2", attr2.Name);
            Assert.IsNull(attr2.Value);

            XPathAttribute attr3 = output.Attributes.Find("attr3");
            Assert.IsNull(attr3);

            XPathAttribute attr4 = output.Attributes.Find("attr4");
            Assert.IsNull(attr4);

            XPathAttribute attr5 = output.Attributes.Find("attr5");
            Assert.IsNull(attr5);

            XPathAttribute attr6 = output.Attributes.Find("attr6");
            Assert.IsNull(attr6);
        }
    }
}
