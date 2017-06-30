using GetFacts.Facts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts
{
    public class ConfigFactory
    {
        private static ConfigFactory uniqueInstance = null;
        private static readonly object _lock_ = new object();

        public static ConfigFactory GetInstance()
        {
            lock(_lock_)
            {
                if(uniqueInstance==null)
                {
                    uniqueInstance = new ConfigFactory();
                }
                return uniqueInstance;
            }
        }

        public List<PageConfig> CreateConfig(string path)
        {
            List<PageConfig> output = new List<PageConfig>();
            string text = File.ReadAllText(path);
            output = JsonConvert.DeserializeObject<List<PageConfig>>(text);
            return output;
        }

        private string AppDir
        {
            get
            {
                return "GetFacts";
            }
        }

        private string DefaultConfigFile
        {
            get
            {
                string location = this.GetType().Assembly.Location;
                string dir = Path.GetDirectoryName(location);
                return Path.Combine(dir, "DefaultConfig.json");
            }
        }

        public string ConfigFile
        {
            get
            {
                return DefaultConfigFile;
            }
        }

        private string DefaultCacheDirectory
        {
            get
            {
                string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = Path.Combine(root, AppDir);
                return path;
            }
        }

        public string CacheDirectory
        {
            get
            {
                string path = DefaultCacheDirectory;
                if( Directory.Exists(path) == false )
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        private string DownloadsListFile
        {
            get
            {
                return "downloads.lst";
            }
        }

        public string DownloadsList
        {
            get
            {
                string dir = CacheDirectory;
                string file = DownloadsListFile;
                return Path.Combine(dir, file);
            }
        }

    }
}
