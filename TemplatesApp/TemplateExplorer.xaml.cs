using GetFacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
            FilesList.Items.Clear();

            try
            {
                string dir = TemplatesDirSelection.SelectedItem as string;
                List<string> templates = TemplateFactory.CreateTemplatesList(dir);
                templates.ForEach(t => FilesList.Items.Add(t));
            }
            catch(DirectoryNotFoundException)
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
        }

        private void Preview(string path)
        {
            string text = string.Empty;

            if (path != null)
            {
                text = File.ReadAllText(path);
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
    }
}
