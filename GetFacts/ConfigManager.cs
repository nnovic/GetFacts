using GetFacts.Facts;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts
{
    public abstract class ConfigManager
    {
        private static ConfigManager uniqueInstance = null;
        private static readonly object _lock_ = new object();

        public static ConfigManager GetInstance()
        {
            lock(_lock_)
            {
                if(uniqueInstance==null)
                {
                    if (IsPortable) uniqueInstance = new PortableAppConfig();
                    else uniqueInstance = new InstalledAppConfig();
                }
                return uniqueInstance;
            }
        }

        public static bool IsPortable
        {
            get
            {
                AssemblyName an = Assembly.GetEntryAssembly().GetName();
                string cb = an.CodeBase;
                string filename= Path.GetFileNameWithoutExtension(cb);
                return filename.EndsWith("portable", StringComparison.InvariantCultureIgnoreCase);
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









        protected string AppName
        {
            get
            {
                return "GetFacts";
            }
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

        /// <summary>
        /// Retourne le chemin d'accès au fichier
        /// de config par défaut.
        /// </summary>
        /// <remarks>Le fichier en lui-même peut ne pas exister!</remarks>
        public abstract string DefaultConfigFile
        {
            get;
        }

        /// <summary>
        /// Retourne le chemin d'accès au fichier
        /// de config courant. Si le fichier de config
        /// courant n'existe pas, la valeur de DefaultConfigFile
        /// sera utilisée.
        /// </summary>
        /// <see cref="DefaultConfigFile"/>
        public abstract string ConfigFile
        {
            get;
        }

        /// <summary>
        /// Retourne le chemin qui, en l'absence d'information
        /// disant le contraire, devra être utilisé pour stocker
        /// le cache de l'appli.
        /// </summary>
        public abstract string DefaultCacheDirectory
        {
            get;
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

        /// <summary>
        /// Chemin dans lequel on cherche les fichiers templates.
        /// Retourne la valeur qui est sauvegardée sur le profil de l'utilisateur.
        /// </summary>
        /// <remarks>Par défaut, TemplatesDirectory vaut DefaultTemplatesDirectory</remarks>
        /// <see cref="DefaultTemplatesDirectory"/>
        /// <seealso cref="TemplateFactory.TemplatesDirectory"/>
        public abstract string TemplatesDirectory
        {
            get;
        }


        /// <summary>
        /// Par défault, les templates seront stockés
        /// dans le sous-répertoire "GetFacts/Templates"
        /// du répertoire système "LocalApplicationData"
        /// de l'utilisateur courant.
        /// </summary>
        public abstract string DefaultTemplatesDirectory
        {
            get;
        }
        
        public List<string> GetMruTemplatesDictories()
        {
            return GetListOfStringsFromRegistry("Templates", "MRU_Locations");
        }

        public void SaveMruTemplatesDirectories(ICollection<string> list)
        {
            SetListOfStringsToRegistry("Templates", "MRU_Locations", list);
        }

        public List<string> GetMruTemplatesUrls()
        {
            return GetListOfStringsFromRegistry("Templates", "MRU_Urls");
        }

        public void SaveMruTemplatesUrls(ICollection<string> list)
        {
            SetListOfStringsToRegistry("Templates", "MRU_Urls", list);
        }

        #endregion

        public WindowPosition WindowPosition
        {
            get
            {
                string s = GetStringFromRegistry(null, "WindowPosition");
                if (string.IsNullOrEmpty(s)) return null;
                return WindowPosition.CreateFrom(s);
            }
            set
            {
                string s = value.ToJson();
                SetStringToRegistry(null, "WindowPosition", s);
            }
        }
        


        #region registry

        RegistryKey GetRegistryKey(string key, bool forWriting)
        {
            string path = string.Format(@"SOFTWARE\{0}\{1}", AppName, key);
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

        /// <summary>
        /// Obtient une string depuis la base de registre
        /// de l'utilisateur pour cette application.
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        string GetStringFromRegistry(string keyPath, string valueName)
        {
            string output = null;

            using (RegistryKey key = GetRegistryKey(keyPath, false))
            {
                if (key != null)
                {
                    output = key.GetValue(valueName) as string;
                }
            }

            return output;
        }

        /// <summary>
        /// Obtient une string depuis la base de registre
        /// de l'utilisateur pour cette application.
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        void SetStringToRegistry(string keyPath, string valueName, string value)
        {            
            using (RegistryKey key = GetRegistryKey(keyPath, true))
            {
                if (key != null)
                {
                    key.SetValue(valueName, value);
                }
            }           
        }

        List<string> GetListOfStringsFromRegistry(string keyPath, string valueName)
        {
            List<string> output = new List<string>();

            using (RegistryKey key = GetRegistryKey(keyPath, false))
            {
                if (key != null)
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
            }

            return output;
        }
                
        #endregion



        internal class InstalledAppConfig:ConfigManager
        {
            /// <summary>
            /// Par défault, les fichiers de configuration seront stockés
            /// dans le sous-répertoire "GetFacts"
            /// du répertoire système "LocalApplicationData"
            /// de l'utilisateur courant.
            /// </summary>
            public override string DefaultConfigFile
            {
                get
                {
                    string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string file = Path.Combine(root, AppName);
                    file = Path.Combine(file, "DefaultConfig.json");
                    return file;
                }
            }

            private string InternalConfigFile
            {
                get
                {
                    string location = this.GetType().Assembly.Location;
                    string dir = Path.GetDirectoryName(location);
                    return Path.Combine(dir, "DefaultConfig.json");
                }
            }

            private void CopyDefaultConfig(string dst)
            {
                string src = InternalConfigFile;
                File.Copy(src, dst, true);
            }

            /// <summary>
            /// Cherche dans la base de registre le chemin du fichier de config.
            /// Si un tel chemin n'est pas défini dans la base de registre, 
            /// la valeur de DefaultConfigFile sera utilisée.
            /// Ensuite, si le fichier de config lui-même n'existe pas, il est
            /// créé par recopie du contenu de InternalConfigFile.
            /// Finalement, le chemin d'accès au fichier de config est retourné.
            /// </summary>
            public override string ConfigFile
            {
                get
                {
                    // essaye de charger la valeur depuis la base de registre:
                    string file = GetStringFromRegistry("Configuration", "Location");

                    // sinon, utiliser la valeur par défaut
                    if (string.IsNullOrEmpty(file))
                    {
                        file = DefaultConfigFile;
                    }

                    // créer le fichier si nécessaire
                    if (File.Exists(file) == false)
                    {
                        CopyDefaultConfig(file);
                    }

                    return file;
                }
            }

            /// <summary>
            /// Par défaut, le cache de l'application sera
            /// stocké dans le sous-répertoire "GetFacts/Cache" 
            /// du répertoire système "LocalApplicationData"
            /// de l'utilisateur courant.
            /// </summary>
            public override string DefaultCacheDirectory
            {
                get
                {
                    string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string path = Path.Combine(root, AppName);
                    path = Path.Combine(path, "Cache");
                    return path;
                }
            }


            /// <summary>
            /// Chemin dans lequel on cherche les fichiers templates.
            /// Retourne la valeur qui est sauvegardée sur le profil de l'utilisateur.
            /// </summary>
            /// <remarks>Par défaut, TemplatesDirectory vaut DefaultTemplatesDirectory</remarks>
            /// <see cref="DefaultTemplatesDirectory"/>
            /// <seealso cref="TemplateFactory.TemplatesDirectory"/>
            public override string TemplatesDirectory
            {
                get
                {
                    // essaye de charger la valeur depuis la base de registre:
                    string dir = GetStringFromRegistry("Templates", "Location");

                    // sinon, utiliser la valeur par défaut
                    if (string.IsNullOrEmpty(dir))
                    {
                        dir = DefaultTemplatesDirectory;
                    }

                    // créer le répertoire si nécessaire
                    if (Directory.Exists(dir) == false)
                    {
                        CopyTemplatesTo(dir);
                    }

                    return dir;
                }
            }

            private void CopyTemplatesTo(string dst)
            {
                string src = InternalTemplatesDirectory;
                IEnumerable<string> files = Directory.EnumerateFiles(src, "*.json", SearchOption.AllDirectories);
                foreach (string absoluteSourcePath in files)
                {
                    string relativeSourcePath = Toolkit.GetRelativePath(absoluteSourcePath, src);
                    string absoluteDestPath = Path.Combine(dst, relativeSourcePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(absoluteDestPath));
                    File.Copy(absoluteSourcePath, absoluteDestPath);
                }
            }

            /// <summary>
            /// Localisation des Templates livrés avec l'application,
            /// dans le répertoire d'installation de l'application.
            /// </summary>
            private string InternalTemplatesDirectory
            {
                get
                {
                    string location = this.GetType().Assembly.Location;
                    string dir = Path.GetDirectoryName(location);
                    string path = Path.Combine(dir, "Templates");
                    return path;
                }
            }

            /// <summary>
            /// Par défault, les templates seront stockés
            /// dans le sous-répertoire "GetFacts/Templates"
            /// du répertoire système "LocalApplicationData"
            /// de l'utilisateur courant.
            /// </summary>
            public override string DefaultTemplatesDirectory
            {
                get
                {
                    string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string path = Path.Combine(root, AppName);
                    path = Path.Combine(path, "Templates");
                    return path;
                }
            }
        }





        internal class PortableAppConfig:ConfigManager
        {

            private string PortableDir
            {
                get
                {
                    Assembly assembly = Assembly.GetEntryAssembly();
                    if (assembly == null)
                        assembly = Assembly.GetExecutingAssembly();

                    AssemblyName assemblyName = assembly.GetName();
                    Uri codeBase = new Uri(assemblyName.CodeBase);

                    string dir = Path.GetDirectoryName(codeBase.AbsolutePath);
                    return dir;
                }
            }

            /// <summary>
            /// Par défault, les fichiers de configuration seront stockés
            /// dans le sous-répertoire "GetFacts"
            /// du répertoire de l'appli.
            /// </summary>
            public override string DefaultConfigFile
            {
                get
                {
                    string file = Path.Combine(PortableDir, "DefaultConfig.json");
                    return file;
                }
            }

            public override string ConfigFile => DefaultConfigFile;

            /// <summary>
            /// Le cache de l'application sera
            /// stocké dans le sous-répertoire "Cache" 
            /// du répertoire de l'appli.
            /// </summary>
            public override string DefaultCacheDirectory
            {
                get
                {
                    string path = Path.Combine(PortableDir, "Cache");
                    return path;
                }
            }

            /// <summary>
            /// Par défault, les templates seront stockés
            /// dans le sous-répertoire "GetFacts/Templates"
            /// du répertoire système "LocalApplicationData"
            /// de l'utilisateur courant.
            /// </summary>
            public override string DefaultTemplatesDirectory
            {
                get
                {
                    string path = Path.Combine(PortableDir, "Templates");
                    return path;
                }
            }

            public override string TemplatesDirectory => DefaultTemplatesDirectory;
        }
    }
}
