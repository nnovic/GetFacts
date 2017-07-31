using GetFacts.Facts;
using Microsoft.Win32;
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

        public static List<PageConfig> Load()
        {
            string path = GetInstance().ConfigFile;
            return GetInstance().LoadConfig(path);
        }

        public static void Save(List<PageConfig> config)
        {
            string path = GetInstance().ConfigFile;
            GetInstance().SaveConfig(path, config);
        }

        public List<PageConfig> LoadConfig(string path)
        {
            List<PageConfig> output = new List<PageConfig>();
            string text = File.ReadAllText(path);
            output = JsonConvert.DeserializeObject<List<PageConfig>>(text);
            return output;
        }

        public void SaveConfig(string path, List<PageConfig> config)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            string text = JsonConvert.SerializeObject(config, Formatting.Indented, settings);
            File.WriteAllText(path, text);
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


        #region

        public string TemplatesDirectory
        {
            get
            {
                return DefaultTemplatesDirectory;
            }
        }

        private string DefaultTemplatesDirectory
        {
            get
            {
                string location = this.GetType().Assembly.Location;
                string dir = Path.GetDirectoryName(location);
                string path = Path.Combine(dir, "Templates");
                return path;
            }
        }
        
        public List<string> GetMruTemplatesDictories()
        {
            return GetListOfStringsFromRegistry("Templates", "MRU");
        }

        public void SaveMruTemplatesDirectories(ICollection<string> list)
        {
            SetListOfStringsToRegistry("Templates", "MRU", list);
        }

        #endregion


        #region registry

        RegistryKey GetRegistryKey(string key, bool forWriting)
        {
            string path = string.Format(@"SOFTWARE\{0}\{1}", AppDir, key);
            if (forWriting)
                return Registry.CurrentUser.CreateSubKey(path);
            return Registry.CurrentUser.OpenSubKey(path);
        }

        void SetListOfStringsToRegistry(string keyPath, string valueName, ICollection<string> list)
        {
            string[] array = list.ToArray();
            using (RegistryKey key = GetRegistryKey(keyPath, true))
            {
                key.SetValue(valueName, array, RegistryValueKind.MultiString);
            }
        }

        List<string> GetListOfStringsFromRegistry(string keyPath, string valueName)
        {
            List<string> output = new List<string>();

            using (RegistryKey key = GetRegistryKey(keyPath, false))
            {
                string[] array = key.GetValue(valueName) as string[];
                if (array != null)
                {
                    foreach (string item in array)
                    {
                        output.Add(item);
                    }
                }
            }

            return output;
        }
                
        #endregion

    }
}
