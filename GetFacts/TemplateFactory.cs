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

        public void SaveTemplate(PageTemplate template, string dst)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            string output = JsonConvert.SerializeObject(template, Formatting.Indented, settings);
            string path = Path.Combine(TemplatesDirectory, dst);
            File.WriteAllText(path, output);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Si path n'est pas rooted, alors path est combiné avec TemplatesDirectory</param>
        /// <returns></returns>
        public PageTemplate GetExistingTemplate(string path)
        {
            return GetJSONTemplate(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Si path n'est pas rooted, alors path est combiné avec TemplatesDirectory</param>
        /// <returns></returns>
        public PageTemplate CreateNewTemplate(string path)
        {
            string absolutePath = path;
            string relativePath = path;

            if (Path.IsPathRooted(path) == false)
            {
                string dir = TemplatesDirectory;
                absolutePath = Path.Combine(dir, path);
            }
            else
            {
                string dir = TemplatesDirectory;
                relativePath = Toolkit.GetRelativePath(absolutePath, dir);
            }

            using (StreamWriter sw = File.CreateText(absolutePath))
            {
                sw.WriteLine("{");
                sw.WriteLine("}");
            }

            return GetJSONTemplate(path);
        }

        /// <summary>
        /// Reads the content of JSON file, whose path is provided in argument.
        /// </summary>
        /// <param name="path">Si path n'est pas rooted, alors path est combiné avec TemplatesDirectory</param>
        /// <returns></returns>
        private PageTemplate GetJSONTemplate(string path)
        {
            string absolutePath = path;
            string relativePath = path;

            if (Path.IsPathRooted(path) == false)
            {
                string dir = TemplatesDirectory;
                absolutePath = Path.Combine(dir, path);
            }
            else
            {
                string dir = TemplatesDirectory;
                relativePath = Toolkit.GetRelativePath(absolutePath, dir);
            }

            PageTemplate output;
            string text = File.ReadAllText(absolutePath);
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
