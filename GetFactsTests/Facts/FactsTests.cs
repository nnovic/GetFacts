using Microsoft.VisualStudio.TestTools.UnitTesting;
using GetFacts.Facts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetFacts.Download;
using System.IO;

namespace GetFacts.Facts.Tests
{
    [TestClass()]
    public class FactsTests
    {
        private class TestableFacts:Facts
        {
            public TestableFacts():base("RotationTestConfig.json")
            {

            }
        }

        private class TestableDownloadManager: DownloadManager
        {
            public TestableDownloadManager():base()
            {
                uniqueInstance = this;
            }

            protected override bool DownloadsFileSupported
            {
                get { return false; }
            }
        }

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
        /// Test of the Facts#More() method.
        /// </summary>
        [TestMethod()]
        public void MoreTest()
        {
            Facts facts = new TestableFacts();
            DownloadManager dm = new TestableDownloadManager();
            dm.Queue( new TestableDownloadTask("site1.html", "00000000-0000-0000-0000-000000000001") );
            Assert.Fail();
        }
    }
}