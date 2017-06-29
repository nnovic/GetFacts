using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GetFacts.Download;
using System.IO;
using GetFactsTests.Facts;
using GetFactsTests.Download;

namespace GetFacts.Facts.Tests
{
    [TestClass()]
    public class RotationTests
    {
        /*private class TestableDownloadManager: DownloadManager
        {
            public TestableDownloadManager():base()
            {
                uniqueInstance = this;
            }

            protected override bool DownloadsFileSupported
            {
                get { return false; }
            }
        }*/

        private class TestableDownloadTask:DownloadTask
        {
            public TestableDownloadTask(string file, string id):base(  
                new Uri(
                    Path.Combine("Cache",file), 
                    UriKind.Relative), 
                Guid.Parse(id) )
            {
            }
        }


        /// <summary>
        /// Test de la méthode Facts#More() dans un
        /// scénario sans piège particulier.
        /// </summary>
        [TestMethod()]
        public void More_testSimple()
        {
            DownloadManager dm = new TestableDownloadManager();
            dm.Queue(new TestableDownloadTask("http://www.site1.com/index.html", "00000000-0000-0000-0000-000000000001"));
            dm.Queue(new TestableDownloadTask("http://www.site2.com/index.html", "00000000-0000-0000-0000-000000000002"));
            Facts facts = new TestableFacts("RotationTestConfig.json");
            facts.Initialize();
            //Assert.Fail();
        }
    }
}