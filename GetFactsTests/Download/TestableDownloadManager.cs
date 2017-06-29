using GetFacts.Download;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFactsTests.Download
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


    /// <summary>
    /// Surcharge la class GetFacts.Download.DownloadManager
    /// afin d'exposer les données normalement privées,
    /// et de pouvoir éditer le cache de téléchargement.
    /// </summary>
    public class TestableDownloadManager:DownloadManager
    {
        public TestableDownloadManager() : base()
        {
            uniqueInstance = this;
        }

        protected override bool DownloadsFileSupported
        {
            get { return false; }
        }
    }
}
