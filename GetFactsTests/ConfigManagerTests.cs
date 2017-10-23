using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GetFacts;
using System.IO;

namespace GetFactsTests
{
    [TestClass]
    public class ConfigManagerTests
    {
        /// <summary>
        /// Vérifie que ConfigManager.DefaultConfigFile est un chemin
        /// absolu et local, càd sans marque d'URI (ex: "file://")
        /// </summary>
        [TestMethod]
        public void DefaultConfigMustNotBeURI()
        {
            ConfigManager localCM = new ConfigManager.InstalledAppConfig();
            ConfigManager portableCM = new ConfigManager.PortableAppConfig();
            Assert.IsTrue(Path.IsPathRooted(localCM.DefaultConfigFile));
            Assert.IsTrue(Path.IsPathRooted(portableCM.DefaultConfigFile));
        }
    }
}
