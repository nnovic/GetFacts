using GetFacts;
using GetFacts.Download;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
        public readonly ObservableCollection<string> MRU = new ObservableCollection<string>();

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

            PreviousSelection.IsEnabled = false;
            NextSelection.IsEnabled = false;
            ElementsCount = 0;
            SelectedIndex = -1;

            UrlInput.ItemsSource = MRU;
            foreach (string dir in ConfigManager.GetInstance().GetMruTemplatesUrls())
            {
                MRU.Add(dir);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //string path = TemplateFactory.GetInstance().TemplatesDirectory;
            //UpdateMRU(path);
            //TemplatesDirSelection.SelectedItem = path;
        }

        private void Workflow_WorkflowUpdated(object sender, EventArgs e)
        {
            if( workflow.IsTemplateDataAvailable != isReady )
            {
                isReady = workflow.IsTemplateDataAvailable;
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
            HighlightedElements.Clear();
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

        private void PageTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Workflow.PageTemplate.PageType = (string)PageTypeSelector.SelectedItem;
            Parser = AbstractParser.NewInstance(Workflow.PageTemplate.PageType);
        }

        private void CharsetSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Encoding selected = (Encoding)e.AddedItems[0];
            Workflow.PageTemplate.Charset = selected.WebName;
        }

        private void UrlInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Workflow.PageTemplate.Reference = UrlInput.Text;
            DeleteDownloadTask();
            BrowseButton.Content = "Load";
        }

        #endregion


        private void DeleteDownloadTask()
        {
            if (Workflow.DownloadTask != null)
            {
                Workflow.DownloadTask.TaskFinished -= DownloadTask_TaskFinished;
                Workflow.DownloadTask = null;
            }
        }

        private void CreateDownloadTask(string url)
        {
            if (Workflow.DownloadTask == null)
            {
                Workflow.PageTemplate.Reference = url;
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

        #region loading the html document

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            PleaseWaitPanel.Visibility = Visibility.Visible;
            UrlBar.IsEnabled = false;
            string url = UrlInput.Text;
            Task.Run(() => { CreateDownloadTask(url); }); // création d'une tâche obligée pour permettre le rafraichissement de l'UI et réllement "voir" le PleaseWaitPanel apparaître.
            UpdateMRU(url);
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
                PleaseWaitPanel.Visibility = Visibility.Hidden;
            }));
        }

        #endregion

        #region navigation dans le code source

        private bool CodeSourceTreeIsCollapsing = false;

        private void CodeSourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(CodeSourceTreeIsCollapsing)
            {
                return;
            }

            TreeViewItem selected = (TreeViewItem)e.NewValue;
            selected.BringIntoView();
            selected.Focus();

            if (PreFilterLock.IsChecked==false)
            {
                AbstractXPathBuilder xpath = Parser.SuggestXPathFor(selected, String.Empty);
                XPathInput2.Text = string.Empty;
                XPathInput1.Text = xpath.ToString();
                ClickHistory.Add(xpath);
            }
            else
            {
                string root = XPathInput1.Text;
                AbstractXPathBuilder xpath = Parser.SuggestXPathFor(selected, root);
                XPathInput2.Text = xpath.ToString();
                ClickHistory.Add(xpath);
            }
        }

        private void XPathInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearHighlights();            
            try
            {
                IList<TextElement> selection = Parser?.SelectFromXPath(XPathInput1.Text, XPathInput2.Text);
                if( (selection != null) && (selection.Count>0) )
                {
                    Highlights(selection);
                    ExpandSourceTreeButOnlyFor(selection);
                }
            }
            catch
            {
            }            
        }

        #endregion

        #region expansion de l'arbre

        private void ExpandSourceTreeButOnlyFor(IList<TextElement> list)
        {
            try
            {
                CodeSourceTreeIsCollapsing = true;

                List<TreeViewItem> expandedItems = new List<TreeViewItem>();

                foreach (TextElement te in list)
                {
                    TreeViewItem tvi = Parser.TextElementToTreeViewItem(te);

                    while (tvi != null)
                    {
                        expandedItems.Add(tvi);
                        tvi.IsExpanded = true;
                        tvi = tvi.Parent as TreeViewItem;
                    }
                }

                foreach (TreeViewItem tvi in CodeSourceTree.Items)
                {
                    CollapseAllChildrenExcept(tvi, expandedItems);
                }
            }
            finally
            {
                CodeSourceTreeIsCollapsing = false;
            }
        }

        private void CollapseAllChildrenExcept(TreeViewItem item, List<TreeViewItem> expandedItems)
        {
            foreach (TreeViewItem child in item.Items)
            {
                if (expandedItems.Contains(child))
                {
                    child.IsExpanded = true;
                    CollapseAllChildrenExcept(child, expandedItems);
                }
                else
                {
                    child.IsExpanded = false;
                }
            }
        }

        #endregion


        #region mise en surbrillance

        private readonly List<TextElement> HighlightedElements = new List<TextElement>();

        /*private readonly List<TextElement> selectedTextElements = new List<TextElement>();
        private int selectedIndex = -1;
        */
        private void ClearHighlights()
        {
            ClearSelection();

            foreach (TextElement te in HighlightedElements)
            {
                te.Background = null;
            }
            HighlightedElements.Clear();
            ElementsCount = 0;
            SelectedIndex = -1;
        }

        private void Highlights(IList<TextElement> list)
        {
            if ((list == null) || (list.Count < 1))
            {
                return;
            }

            HighlightedElements.AddRange(list);
            foreach (TextElement te in list)
            {
                te.Background = Brushes.Orange;
            }

            ElementsCount = HighlightedElements.Count;

            TextElement first = list[0];
            int index = HighlightedElements.IndexOf(first);
            Select(index);
        }

        private int ElementsCount
        {
            set
            {
                SelectionCount.Text = value.ToString();
            }
        }

        #endregion

        #region selection among highlighted elements 

        private int SelectedIndex
        {
            set
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Selected: ");
                if(value<0)
                {
                    sb.Append("none");
                }
                else
                {
                    sb.AppendFormat("{0}/{1}", value+1, HighlightedElements.Count);
                }
                SelectionIndex.Text = sb.ToString();
            }
        }

        private TextElement SelectedElement = null;

        private void ClearSelection()
        {
            if(SelectedElement!=null)
            {
                if( HighlightedElements.Contains(SelectedElement))
                {
                    SelectedElement.Background = Brushes.Orange;
                }
                else
                {
                    SelectedElement.Background = null;
                }
                SelectedElement = null;
                SelectedIndex = -1;
            }
        }

        private void Select(int index)
        {
            ClearSelection();

            if ((index >= 0) && (index < HighlightedElements.Count))
            {
                SelectedElement = HighlightedElements[index];
                SelectedElement.Background = Brushes.Yellow;
                SelectedElement.BringIntoView();
            }

            NextSelection.IsEnabled = (index>=0) && ((index + 1) < HighlightedElements.Count); 
            PreviousSelection.IsEnabled = (index > 0) && (index < HighlightedElements.Count);
            SelectedIndex = index;
        }

        private void NextSelection_Click(object sender, RoutedEventArgs e)
        {
            int index = HighlightedElements.IndexOf(SelectedElement);
            Select(++index);
        }

        private void PreviousSelection_Click(object sender, RoutedEventArgs e)
        {
            int index = HighlightedElements.IndexOf(SelectedElement);
            Select(--index);
        }


        #endregion


        /// <summary>
        /// Place l'URL passé en argument en
        /// tête de la liste des URLs les plus
        /// utilisées.
        /// </summary>
        /// <param name="url"></param>
        private void UpdateMRU(string url)
        {
            UrlInput.BeginInit();
            try
            {
                if (MRU.Contains(url))
                {
                    MRU.Remove(url);
                }
                else
                {
                    while (MRU.Count > 4)
                    {
                        MRU.RemoveAt(4);
                    }
                }
                MRU.Insert(0, url);
                ConfigManager.GetInstance().SaveMruTemplatesUrls(MRU);
            }
            finally
            {
                UrlInput.EndInit();
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlInput.Text;
            Process.Start(url);

        }

        private void ClickHistory_XPathEntryDoubleClick(object sender, AbstractXPathBuilder e)
        {
            if ( e!=null)
                XPathInput1.Text = e.ToString();
        }

    }
}
