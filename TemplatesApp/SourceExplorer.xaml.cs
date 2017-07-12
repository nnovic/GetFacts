using GetFacts.Download;
using GetFacts.Parse;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TemplatesApp
{
    /// <summary>
    /// Logique d'interaction pour SourceExplorer.xaml
    /// </summary>
    public partial class SourceExplorer : UserControl
    {


        public SourceExplorer()
        {
            InitializeComponent();
        }


        #region configuration

        private PageTemplate pageTemplate;

        public PageTemplate PageTemplate
        {
            get
            {
                return pageTemplate;
            }
            set
            {
                pageTemplate = value;
                Parser = pageTemplate.GetParser();
                UrlInput.Text = pageTemplate.Reference;
            }
        }


        public AbstractParser Parser
        {
            get;
            private set;
        }


        public Encoding Encoding
        {
            get; set;
        }

        #endregion


        #region loading the html document

        private DownloadTask downloadTask = null;

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlInput.Text;
            UrlBar.IsEnabled = false;

            if(downloadTask==null)
            {
                Uri uri = new Uri(url);
                downloadTask = DownloadManager.GetInstance().FindOrQueue(uri, Parser.MostProbableFileExtension);
                downloadTask.TaskFinished += DownloadTask_TaskFinished;
                downloadTask.TriggerIfTaskFinished();
            }
            else
            {
                downloadTask.Reload();
            }
        }

        private void DownloadTask_TaskFinished(object sender, EventArgs e)
        {
            if (downloadTask.Status == DownloadTask.DownloadStatus.Completed)
            {
                Parser.Load(downloadTask.LocalFile, Encoding);
                Dispatcher.Invoke(() => 
                {
                    CodeSourceView.Document = Parser.SourceCode;
                    CodeSourceTree.Items.Add(Parser.SourceTree);
                    //UrlBar.IsEnabled = true;
                });
            }
        }


        #endregion

        private void CodeSourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selected = (TreeViewItem)e.NewValue;
            selected.BringIntoView();
            selected.Focus();

            string xpath = Parser.SuggestXPathFor(selected);
            XPathInput.Text = xpath;
        }
    }
}
