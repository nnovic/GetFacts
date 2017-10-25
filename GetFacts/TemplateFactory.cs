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
                    uniqueInstance = new TemplateFactory()
                    {
                        TemplatesDirectory = ConfigManager.GetInstance().TemplatesDirectory }
                    ;
                }
                return uniqueInstance;
            }
        }

        /// <summary>
        /// Destruction de cet objet:
        /// - s'assurer que toutes les notifications poussées dans
        ///   NotificationSystem par cet objet soient supprimées.
        /// </summary>
        ~TemplateFactory()
        {
            NotificationSystem.GetInstance().RemoveAll(this);
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
        /// <remarks>Ajoute une notification dans NotificationSystem 
        /// si problème d'accès au fichier, mais ne bloque pas les exceptions.</remarks>
        /// <seealso cref="NotificationKeys.TemplateFileError"/>
        public PageTemplate GetExistingTemplate(string path)
        {
            var notification = new NotificationSystem.Notification(this,
                (int)NotificationKeys.TemplateFileError)
            {
                Title = path,
                Description = "Template file error."
            };

            try
            {
                PageTemplate output = GetJSONTemplate(path);
                NotificationSystem.GetInstance().Remove(notification);
                return output;
            }
            catch
            {
                NotificationSystem.GetInstance().Add(notification);
                throw;
            }
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
        /// Chemin dans lequel on cherche les fichiers templates.
        /// Initialisé avec la valeur de ConfigFactory.GetInstance().TemplatesDirectory.
        /// Toute modification ultérieure de cette propriété n'est pas répercutée sur le contenu
        /// de ConfigFactory.GetInstance().TemplatesDirectory.
        /// </summary>
        /// <see cref="ConfigManager.TemplatesDirectory"/>
        public string TemplatesDirectory
        {
            get;set;
        }

        public List<string> CreateTemplatesList()
        {
            return CreateTemplatesList(TemplatesDirectory);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        /// <remarks>Si "dir" est null ou vide, retourne une
        /// liste vide.</remarks>
        public static List<string> CreateTemplatesList(string dir)
        {
            List<string> output = new List<string>();

            if (!string.IsNullOrEmpty(dir) )
            {
                foreach (string path in Directory.EnumerateFiles(dir, "*.json", SearchOption.AllDirectories))
                {
                    string file = Toolkit.GetRelativePath(path, dir);
                    output.Add(file);
                }
            }

            return output;
        }


        /// <summary>
        /// Enumération des clés que cette classe utilise
        /// pour insérer/supprimer des notifications dans
        /// NotificationSystem.
        /// </summary>
        enum NotificationKeys
        {
            /// <summary>
            /// Erreur durant l'accès au fichier template.
            /// Fichier manquant ? Mauvais formattage ?
            /// </summary>
            /// <see cref="GetExistingTemplate(string)"/>
            TemplateFileError
        }
    }
}
