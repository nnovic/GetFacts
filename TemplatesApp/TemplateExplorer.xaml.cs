using GetFacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Documents;
using System.Reflection;
using System.Collections.ObjectModel;

namespace TemplatesApp
{
    /// <summary>
    /// Logique d'interaction pour TemplateExplorer.xaml
    /// </summary>
    public partial class TemplateExplorer : UserControl
    {

        private Workflow workflow;
        public readonly ObservableCollection<string> MRU = new ObservableCollection<string>();

        public TemplateExplorer()
        {
            InitializeComponent();
        }

        internal Workflow Workflow
        {
            set
            {
                workflow = value;
                workflow.WorkflowUpdated += Workflow_WorkflowUpdated;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string path = TemplateFactory.GetInstance().TemplatesDirectory;
            UpdateMRU(path);
            TemplatesDirSelection.SelectedItem = path;
        }

        private void TemplatesDirSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string path = TemplatesDirSelection.SelectedItem as string;
            TemplatesDirSelected(path);
        }

        private void TemplatesDirSelected(string path)
        {
            try
            {
                FilesList.TemplatesDirectory = path;
                TemplateFactory.GetInstance().TemplatesDirectory = path;
            }
            catch(ArgumentException)
            {
                MessageBoxResult res = MessageBox.Show(string.Format("Error while setting directory: \"{0}\". Do you want to remove it from the list ?", path), "Invalid selection", MessageBoxButton.YesNo, MessageBoxImage.Error);
                FilesList.TemplatesDirectory = null;
            }
        }

        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string dir = TemplatesDirSelection.SelectedItem as string;
                string template = FilesList.SelectedItem as string;
                string path = Path.Combine(dir, template);
                Preview(path);
            }
            catch
            {
                Preview(null);
            }
        }


        public string SelectedTemplate
        {
            get
            {
                string template = FilesList.SelectedItem as string;
                return template;
            }
            internal set
            {
                string dir = TemplatesDirSelection.SelectedItem as string;
                string template = value;
                template = Toolkit.GetRelativePath(template, dir);
                FilesList.SelectedItem = template;
            }
        }

        private void Preview(string absolutePath)
        {
            string text = string.Empty;

            if (absolutePath != null)
            {
                text = File.ReadAllText(absolutePath);
            }

            Dispatcher.Invoke( ()=> 
            {
                try
                {
                    Run r = new Run(text);
                    Paragraph p = new Paragraph(r);
                    FlowDocument fd = new FlowDocument(p);
                    JsonPreview.Document = fd;
                }
                catch
                {
                }
            });
        }

        private void FilesList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //TODO
        }

        private void Workflow_WorkflowUpdated(object sender, EventArgs e)
        {
            // no action required.
        }

        private void SelectTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            string template = FilesList.SelectedItem as string;
            workflow.TemplateFile = template;
        }

        private void ChangeDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog())
            {
                dlg.Title = "My Title";
                dlg.IsFolderPicker = true;
                dlg.InitialDirectory = @"C:\Users\alexandre\Documents\GetFacts\Templates\Templates";

                dlg.AddToMostRecentlyUsedList = false;
                dlg.AllowNonFileSystemItems = false;
                dlg.DefaultDirectory = TemplateFactory.GetInstance().TemplatesDirectory;
                dlg.EnsureFileExists = true;
                dlg.EnsurePathExists = true;
                dlg.EnsureReadOnly = false;
                dlg.EnsureValidNames = true;
                dlg.Multiselect = false;
                dlg.ShowPlacesList = true;

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string folder = dlg.FileName;
                    UpdateMRU(folder);
                    TemplatesDirSelection.SelectedItem = folder;
                }
            }
        }        

        /// <summary>
        /// Place le répertoire passé en argument en
        /// tête de la liste des répertoires les plus
        /// utilisés (il s'agit des répertoires dans
        /// lesquels ce composant va rechercher les
        /// fichiers templates).
        /// </summary>
        /// <param name="folder"></param>
        private void UpdateMRU(string folder)
        {
            TemplatesDirSelection.BeginInit();
            try
            {
                if (MRU.Contains(folder))
                {
                    MRU.Remove(folder);
                }
                else
                {
                    while (MRU.Count > 4)
                    {
                        MRU.RemoveAt(4);
                    }
                }
                MRU.Insert(0, folder);
                ConfigManager.GetInstance().SaveMruTemplatesDirectories(MRU);
            }
            finally
            {
                TemplatesDirSelection.EndInit();
            }
        }

        private void NewTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonSaveFileDialog())
            {
                dlg.Title = "Create new template file";
                dlg.InitialDirectory = (string)TemplatesDirSelection.SelectedItem;

                dlg.AddToMostRecentlyUsedList = false;
                dlg.DefaultDirectory = TemplateFactory.GetInstance().TemplatesDirectory;
                dlg.EnsurePathExists = true;
                dlg.EnsureReadOnly = false;
                dlg.EnsureValidNames = true;
                dlg.ShowPlacesList = true;
                dlg.DefaultExtension = ".json";
                dlg.Filters.Add(new CommonFileDialogFilter("json", ".json"));
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string path = dlg.FileName;
                    TemplateFactory.GetInstance().CreateNewTemplate(path);
                    //TODO: File system watcher instead of: RefreshFilesList();
                    SelectedTemplate = path;
                    FilesList.Focus();
                }
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            TemplatesDirSelection.ItemsSource = MRU;
            foreach(string dir in ConfigManager.GetInstance().GetMruTemplatesDictories())
            {
                MRU.Add(dir);
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilesList.SearchPattern = FilterTextBox.Text;
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Text = null;
        }
    }
}
