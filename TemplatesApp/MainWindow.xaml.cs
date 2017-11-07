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
            workflow.WorkflowUpdated += Workflow_WorkflowUpdated;

            ExploreTab.IsEnabledChanged += ExploreTab_IsEnabledChanged;

            // Force early initialization of the download manager
            DownloadManager.GetInstance();

            workflow.OnWorkflowUpdated();
        }

        private void ExploreTab_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            if((oldValue==false) && (newValue==true) )
            {
                ExploreTab.IsSelected = true;
            }
        }

        private void Workflow_WorkflowUpdated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                SelectTab.IsEnabled = true;
                ExploreTab.IsEnabled = workflow.IsTemplateDataAvailable;
                EditTab.IsEnabled = workflow.IsPageDataAvailable;
            });
            
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DownloadManager.GetInstance().Stop();
        }

    }
}
