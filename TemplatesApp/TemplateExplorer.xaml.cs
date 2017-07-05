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
        public TemplateExplorer()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string path = TemplateFactory.GetInstance().TemplatesDirectory;
            TemplatesDirSelection.Items.Add(path);
            TemplatesDirSelection.SelectedItem = path;
        }

        private void TemplatesDirSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir = TemplatesDirSelection.SelectedItem as string;
            List<string> templates = TemplateFactory.CreateTemplatesList(dir);
            templates.ForEach(t => FilesList.Items.Add(t));
        }

        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dir = TemplatesDirSelection.SelectedItem as string;
            string template = FilesList.SelectedItem as string;
            string path = Path.Combine(dir, template);
            Preview(path);         
        }

        private void Preview(string path)
        {
            Dispatcher.Invoke( ()=> 
            {
                try
                {
                    //FlowDocument fd = Toolkit.JSonToFlowDocument(path);
                    string text = File.ReadAllText(path);
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
    }
}
