using GetFacts.Parse;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace GetFacts
{
    public class TemplateFactory
    {
        private static TemplateFactory uniqueInstance = null;
        private readonly static object _lock_ = new object();


        public static TemplateFactory GetInstance()
        {
            lock(_lock_)
            {
                if(uniqueInstance==null)
                {
                    uniqueInstance = new TemplateFactory();
                }
                return uniqueInstance;
            }
        }

        public PageTemplate GetTemplate(string path)
        {
            return GetJSONTemplate(path);
        }

        /// <summary>
        /// Reads the content of JSON file, whose path is provided in argument.
        /// The path MUST BE relative to the "Templates" directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private PageTemplate GetJSONTemplate(string path)
        {
            PageTemplate output;
            string dir = TemplatesDirectory;
            path = Path.Combine(dir, path);
            string text = File.ReadAllText(path);
            output = JsonConvert.DeserializeObject<PageTemplate>(text);
            return output;
        }

        /// <summary>
        /// returns ConfigFactory.GetInstance().TemplatesDirectory
        /// </summary>
        public string TemplatesDirectory
        {
            get { return ConfigFactory.GetInstance().TemplatesDirectory; }
        }

        public List<string> CreateTemplatesList()
        {
            return CreateTemplatesList(TemplatesDirectory);
        }


        public static List<string> CreateTemplatesList(string dir)
        {
            List<string> output = new List<string>();
            foreach (string path in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                string file = Toolkit.GetRelativePath(path, dir);
                output.Add(file);
            }
            return output;
        }
    }
}
