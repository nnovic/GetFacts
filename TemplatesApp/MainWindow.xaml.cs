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
            
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            TemplateSelection.Workflow = workflow;
            SourceExplorer.Workflow = workflow;
            TemplateEditor.Workflow = workflow;
            SaveTemplate.Workflow = workflow;
            workflow.WorkflowUpdated += Workflow_WorkflowUpdated;

            // Force early initialization of the download manager
            DownloadManager.GetInstance();

            workflow.OnWorkflowUpdated();
        }

        private void Workflow_WorkflowUpdated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                SelectTab.IsEnabled = true;
                ExploreTab.IsEnabled = workflow.IsReadyForSourceExplorer;
                EditTab.IsEnabled = workflow.IsReadyForTemplateEditor;
                SaveTab.IsEnabled = workflow.IsReadyToSaveTemplate;
            });
            
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DownloadManager.GetInstance().Stop();
        }

    }
}
