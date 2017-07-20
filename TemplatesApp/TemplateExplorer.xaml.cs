using GetFacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Documents;
using System.Reflection;

namespace TemplatesApp
{
    /// <summary>
    /// Logique d'interaction pour TemplateExplorer.xaml
    /// </summary>
    public partial class TemplateExplorer : UserControl
    {

        private Workflow workflow;

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
            TemplatesDirSelection.Items.Add(path);
            TemplatesDirSelection.SelectedItem = path;
        }

        private void TemplatesDirSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFilesList();
        }

        private void RefreshFilesList()
        {            
            try
            {
                FilesList.Items.Clear();
                string dir = TemplatesDirSelection.SelectedItem as string;
                List<string> templates = TemplateFactory.CreateTemplatesList(dir);
                templates.ForEach(t => FilesList.Items.Add(t));
            }
            catch (DirectoryNotFoundException)
            {
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
                    if (TemplatesDirSelection.Items.Contains(folder) == false)
                    {
                        TemplatesDirSelection.Items.Add(folder);
                    }
                    TemplatesDirSelection.SelectedItem = folder;
                }
            }
        }

        private void NewTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonSaveFileDialog())
            {
                dlg.Title = "My Title";
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
                    RefreshFilesList();
                    SelectedTemplate = path;
                    FilesList.Focus();
                }
            }
        }
    }
}
