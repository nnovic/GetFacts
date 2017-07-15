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
                string pageType = pageTemplate.PageType;
                PageTypeSelector.SelectedValue = pageType;
                CharsetSelector.SelectedValue = pageTemplate.Charset;
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
            get;
            private set;
        }

        public string Url
        {
            get;
            private set;
        }

        #endregion


        #region loading the html document

        private DownloadTask downloadTask = null;

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Url = UrlInput.Text;
            UrlBar.IsEnabled = false;

            if(downloadTask==null)
            {
                Uri uri = new Uri(Url);
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
                    OnPageLoaded();
                });
                
            }
        }

        #endregion

        public class PageLoadedEventArgs : EventArgs
        {
            public string Url { get; internal set; }
            public AbstractParser Parser { get; internal set; }
            public Encoding Encoding { get; internal set; }
        }

        public event EventHandler<PageLoadedEventArgs> PageLoaded;

        private void OnPageLoaded()
        {
            PageLoadedEventArgs plea = new PageLoadedEventArgs() { Url = Url, Parser=Parser, Encoding=Encoding };
            PageLoaded?.Invoke(this, plea);
        }


        private void CodeSourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selected = (TreeViewItem)e.NewValue;
            selected.BringIntoView();
            selected.Focus();

            string xpath = Parser.SuggestXPathFor(selected);
            XPathInput.Text = xpath;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            PageTypeSelector.Items.Clear();
            foreach (string type in AbstractParser.AvailableParsers())
            {
                PageTypeSelector.Items.Add(type);
            }

            CharsetSelector.Items.Clear();
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                CharsetSelector.Items.Add(ei.GetEncoding());
            }
        }

        private void PageTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Parser = new HtmlParser();
        }

        private void CharsetSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selectedEncoding = CharsetSelector.SelectedItem;
            Encoding = selectedEncoding as Encoding;
        }
    }
}
