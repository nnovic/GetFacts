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
        private string templateFile;
        private PageTemplate pageTemplate;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void TemplateExplorer_TemplateSelectionChanged(object sender, TemplateExplorer.TemplateSelectionChangedEventArges e)
        {
            SelectTemplateButton.IsEnabled = (e.Path != null);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {            
            ExploreTab.IsEnabled = false;
            EditTab.IsEnabled = false;
            SaveTab.IsEnabled = false;

            SelectTemplateButton.IsEnabled = false;
            CreateTemplateButton.IsEnabled = false;

            // Force early initialization of the download manager
            DownloadManager.GetInstance();
        }

        private void SelectTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            ExploreTab.IsEnabled = true;
            EditTab.IsEnabled = true;
            SaveTab.IsEnabled = false;

            templateFile = TemplateSelection.SelectedTemplate;
            pageTemplate = TemplateFactory.GetInstance().GetTemplate(templateFile);

            SourceExplorer.PageTemplate = pageTemplate;
            TabControl.SelectedItem = ExploreTab;
        }
    }
}
