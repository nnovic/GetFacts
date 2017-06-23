using GetFacts.Parse;
using Newtonsoft.Json;
using System.IO;
using System;

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
            path = Path.Combine("Templates", path);
            string text = File.ReadAllText(path);
            output = JsonConvert.DeserializeObject<PageTemplate>(text);
            return output;
        }

    }
}
