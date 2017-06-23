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
    }
}
