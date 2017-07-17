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
        private AbstractParser parser = null;

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
                if ( isReady )
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


        private void ClearSourceExplorer()
        {
            CodeSourceView.Document = null;
            CodeSourceTree.Items.Clear();
            Parser?.Clear();
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

        public AbstractParser Parser
        {
            get
            {
                return parser;
            }
            set
            {
                ClearSourceExplorer();
                parser = value;
                InitSourceExplorer();
            }
        }
        
        #endregion


        #region loading the html document

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            UrlBar.IsEnabled = false;            

            if(Workflow.DownloadTask==null)
            {
                Uri uri = new Uri(Workflow.PageTemplate.Reference);
                Workflow.DownloadTask = DownloadManager.GetInstance().FindOrQueue(uri, Parser.MostProbableFileExtension);
                Workflow.DownloadTask.TaskFinished += DownloadTask_TaskFinished;
                Workflow.DownloadTask.TriggerIfTaskFinished();
            }
            else
            {
                Workflow.DownloadTask.Reload();
            }
        }

        private void DownloadTask_TaskFinished(object sender, EventArgs e)
        {
            if (Workflow.DownloadTask.Status == DownloadTask.DownloadStatus.Completed)
            {                
                Dispatcher.Invoke(() => 
                {
                    Parser.Load(Workflow.DownloadTask.LocalFile, Workflow.PageTemplate.Encoding);
                    CodeSourceView.Document = Parser.SourceCode;
                    CodeSourceTree.Items.Clear();
                    CodeSourceTree.Items.Add(Parser.SourceTree);                    
                });
            }

            Dispatcher.BeginInvoke((Action)(() =>
            {
                UrlBar.IsEnabled = true;
                BrowseButton.Content = "Reload";
            }));
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

        private void PageTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Workflow.PageTemplate.PageType = (string)PageTypeSelector.SelectedItem;
            Parser = AbstractParser.NewInstance(Workflow.PageTemplate.PageType);
        }

        private void CharsetSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Workflow.PageTemplate.Charset = (string)CharsetSelector.SelectedValue;
        }

        private void UrlInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Workflow.PageTemplate.Reference = UrlInput.Text;
            Workflow.DownloadTask = null;
            BrowseButton.Content = "Browse";
        }
    }
}
