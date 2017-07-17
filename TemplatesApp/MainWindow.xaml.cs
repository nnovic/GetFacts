using GetFacts;
using GetFacts.Download;
using GetFacts.Parse;
using System;
using System.Windows;
using System.Windows.Controls;

namespace TemplatesApp
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Workflow workflow = new Workflow();

        public MainWindow()
        {
            InitializeComponent();
            workflow.WorkflowUpdated += Workflow_WorkflowUpdated;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            /*ExploreTab.IsEnabled = false;
            EditTab.IsEnabled = false;
            SaveTab.IsEnabled = false;

            SelectTemplateButton.IsEnabled = false;
            CreateTemplateButton.IsEnabled = false;
            */

            TemplateSelection.Workflow = workflow;
            SourceExplorer.Workflow = workflow;

            // Force early initialization of the download manager
            DownloadManager.GetInstance();

            workflow.OnWorkflowUpdated();
        }

        private void Workflow_WorkflowUpdated(object sender, EventArgs e)
        {
            SelectTab.IsEnabled = true;
            ExploreTab.IsEnabled = workflow.IsReadyForSourceExplorer;
            EditTab.IsEnabled = false;
            SaveTab.IsEnabled = false;
        }

        private void SelectTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            /*ExploreTab.IsEnabled = true;
            EditTab.IsEnabled = false;

            templateFile = TemplateSelection.SelectedTemplate;
            pageTemplate = TemplateFactory.GetInstance().GetTemplate(templateFile);

            SourceExplorer.PageTemplate = pageTemplate;
            TemplateEditor.PageTemplate = pageTemplate;
            TabControl.SelectedItem = ExploreTab;*/
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DownloadManager.GetInstance().Stop();
        }

        private void SourceExplorer_PageLoaded(object sender, SourceExplorer.PageLoadedEventArgs e)
        {
            //EditTab.IsEnabled = true;
            //TemplateEditor.Url = e.Url;
            //TemplateEditor.Parser = e.Parser;
        }
    }
}
