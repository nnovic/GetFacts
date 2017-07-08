using GetFacts.Download;
using GetFacts.Parse;
using System;
using System.IO;
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
            Parser = new HtmlParser();
        }


        #region configuration

        public AbstractParser Parser
        {
            get; set;
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
                    //UrlBar.IsEnabled = true;
                });
            }
        }

        #endregion
      

    }
}
