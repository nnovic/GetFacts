using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GetFacts
{
    /// <summary>
    /// Suivez les étapes 1a ou 1b puis 2 pour utiliser ce contrôle personnalisé dans un fichier XAML.
    ///
    /// Étape 1a) Utilisation de ce contrôle personnalisé dans un fichier XAML qui existe dans le projet actif.
    /// Ajoutez cet attribut XmlNamespace à l'élément racine du fichier de balisage où il doit 
    /// être utilisé :
    ///
    ///     xmlns:MyNamespace="clr-namespace:GetFacts"
    ///
    ///
    /// Étape 1b) Utilisation de ce contrôle personnalisé dans un fichier XAML qui existe dans un autre projet.
    /// Ajoutez cet attribut XmlNamespace à l'élément racine du fichier de balisage où il doit 
    /// être utilisé :
    ///
    ///     xmlns:MyNamespace="clr-namespace:GetFacts;assembly=GetFacts"
    ///
    /// Vous devrez également ajouter une référence du projet contenant le fichier XAML
    /// à ce projet et regénérer pour éviter des erreurs de compilation :
    ///
    ///     Cliquez avec le bouton droit sur le projet cible dans l'Explorateur de solutions, puis sur
    ///     "Ajouter une référence"->"Projets"->[Recherchez et sélectionnez ce projet]
    ///
    ///
    /// Étape 2)
    /// Utilisez à présent votre contrôle dans le fichier XAML.
    ///
    ///     <MyNamespace:TemplatesListBox/>
    ///
    /// </summary>
    public class TemplatesListBox : ListBox
    {
        static TemplatesListBox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(TemplatesListBox), new FrameworkPropertyMetadata(typeof(TemplatesListBox)));
        }

        ~TemplatesListBox()
        {
            fileSystemWatcher.Dispose();
        }

        #region FilesystemWatcher

        private readonly FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

        #endregion


        #region Selected Item

        private object deferredSelectedItem = null;

        public new object SelectedItem
        {
            get
            {
                return base.SelectedItem;
            }
            set
            {
                lock (this)
                {
                    deferredSelectedItem = value;
                    base.SelectedItem = value;
                }
            }
        }

        #endregion

        #region List of templates

        private List<string> _templates = new List<string>();
        private string _searchPattern = string.Empty;
        
        public List<string> Templates
        {
            get
            {
                return _templates;
            }
            private set
            {
                _templates = value;
                RefreshItems();
            }
        }

        public string SearchPattern
        {
            get
            {
                return _searchPattern;
            }
            set
            {

                _searchPattern = ValidateAndExpand(value);
                RefreshItems();
            }
        }

        private string ValidateAndExpand(string pattern)
        {
            if ( string.IsNullOrEmpty(pattern) )
                return string.Empty;

            pattern = Regex.Escape(pattern);
            pattern = pattern.Replace("\\?", ".");
            pattern = pattern.Replace("\\*", ".*");
            return pattern;
        }

        private void RefreshItems()
        {
            object selected = base.SelectedItem;

            Items.Clear();
            Regex pattern = new Regex(SearchPattern);
            foreach(string t in Templates)
            {
                if (pattern.IsMatch(t) )
                {
                    Items.Add(t);
                }
            }

            if(deferredSelectedItem!=null)
            {
                base.SelectedItem = deferredSelectedItem;
                deferredSelectedItem = null;
            }
            else if( selected!=null )
            {
                base.SelectedItem = selected;
            }
        }
        
        #endregion



        #region TemplatesDirectoryProperty

        static FrameworkPropertyMetadata propertymetadata = new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, 
            new PropertyChangedCallback(TemplatesDirectory_PropertyChanged),
            new CoerceValueCallback(TemplatesDirectory_CoerceValue),
            false, 
            UpdateSourceTrigger.PropertyChanged);

        public static readonly DependencyProperty TemplatesDirectoryProperty = DependencyProperty.Register(
            "TemplatesDirectory", 
            typeof(string), 
            typeof(TemplatesListBox),
            propertymetadata,
            new ValidateValueCallback(TemplatesDirectory_Validate));
        
        public string TemplatesDirectory
        {
            get
            {
                return this.GetValue(TemplatesDirectoryProperty) as string;
            }
            set
            {
                this.SetValue(TemplatesDirectoryProperty, value);
                fileSystemWatcher.Path = value;
                fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private static void TemplatesDirectory_PropertyChanged(
            DependencyObject dobj,
            DependencyPropertyChangedEventArgs e)
        {
            TemplatesListBox lbox = (TemplatesListBox)dobj;
            string oldPath = (string)e.OldValue;
            string newPath = (string)e.NewValue;
            lbox.Templates = TemplateFactory.CreateTemplatesList(newPath);
        }       

        private static object TemplatesDirectory_CoerceValue(
            DependencyObject dobj, 
            object Value)
        {
            return Value;
        }

        private static bool TemplatesDirectory_Validate(object Value)
        {
            string path = Value as string;
            if (string.IsNullOrEmpty(path))
                return true;
            return Directory.Exists(path);
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Created += FileSystemWatcher_Changed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            fileSystemWatcher.Renamed += FileSystemWatcher_Changed;
            base.OnInitialized(e);
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    Templates = TemplateFactory.CreateTemplatesList(TemplatesDirectory);
                }
                catch
                {
                }
            }), null);
        }
    }
}
