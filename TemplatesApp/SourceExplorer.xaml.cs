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
    /// Le principal point d'entrée pour ce panneau est
    /// la propriété PageTemplate. Lorsque cette 
    /// propriété est modifiée avec une nouvelle
    /// valeur, c'est tout le contenu du panneau qui est
    /// modifié.
    /// </summary>
    public partial class SourceExplorer : UserControl
    {
        private Workflow workflow = null;
        private bool isReady = false;

        public SourceExplorer()
        {
            InitializeComponent();
        }

        internal Workflow Workflow
        {
            get
            {
                return workflow;
            }
            set
            {
                workflow = value;
                workflow.WorkflowUpdated += Workflow_WorkflowUpdated;
            }
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

        private void Workflow_WorkflowUpdated(object sender, EventArgs e)
        {
            if( workflow.IsReadyForSourceExplorer != isReady )
            {
                isReady = workflow.IsReadyForSourceExplorer;
                if (  isReady )
                {
                    InitSourceExplorer();
                }
                else
                {
                    ClearSourceExplorer();
                }
            }
        }











        #region configuration

        /*private PageTemplate pageTemplate;

        public PageTemplate PageTemplate
        {
            get
            {
                return pageTemplate;
            }
            set
            {
                if (pageTemplate != value)
                {
                    ClearTemplate();
                    pageTemplate = value;
                    InitTemplate();                    
                }
            }
        }
        */
        private void ClearSourceExplorer()
        {
            CodeSourceView.Document = null;
            CodeSourceTree.Items.Clear();
            //Parser?.Clear();
        }

        private void InitSourceExplorer()
        {
            string pageType = Workflow.PageTemplate.PageType;
            PageTypeSelector.SelectedValue = pageType;

            string charset = Workflow.PageTemplate.Charset;
            CharsetSelector.SelectedValue = charset;

            string url = Workflow.PageTemplate.Reference;
            UrlInput.Text = url;
        }


        /*public AbstractParser Parser
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
        }*/

        #endregion


        #region loading the html document

        private DownloadTask downloadTask = null;

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            UrlBar.IsEnabled = false;

            Workflow.PageTemplate.Reference= UrlInput.Text;
            Workflow.PageTemplate.Charset = (string)CharsetSelector.SelectedValue;
            Workflow.PageTemplate.PageType = (string)PageTypeSelector.SelectedItem;



            if(downloadTask==null)
            {
                Uri uri = new Uri(Workflow.PageTemplate.Reference);
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
                /*Parser.Load(downloadTask.LocalFile, Encoding);
                Dispatcher.Invoke(() => 
                {
                    CodeSourceView.Document = Parser.SourceCode;
                    CodeSourceTree.Items.Add(Parser.SourceTree);
                    //UrlBar.IsEnabled = true;
                    OnPageLoaded();
                });
                */
            }
        }

        #endregion

        private void CodeSourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            /*TreeViewItem selected = (TreeViewItem)e.NewValue;
            selected.BringIntoView();
            selected.Focus();

            string xpath = Parser.SuggestXPathFor(selected);
            XPathInput.Text = xpath;*/
        }


    }
}
