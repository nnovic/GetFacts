using GetFacts.Download;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Text;
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
            Workflow.PageTemplate.Reference = UrlInput.Text;
            Workflow.DownloadTask = null;
            BrowseButton.Content = "Browse";
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

        #region navigation dans le code source

        private void CodeSourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selected = (TreeViewItem)e.NewValue;
            selected.BringIntoView();
            selected.Focus();

            string xpath = Parser.SuggestXPathFor(selected);
            XPathInput.Text = xpath;
        }

        private void XPathInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearHighlights();

            try
            {
                string xpath = XPathInput.Text;
                IList<TextElement> selection = Parser?.SelectFromXPath(xpath);                
                if( (selection != null) && (selection.Count>0) )
                {
                    Highlights(selection);
                    XPathInput.Foreground = Brushes.Green;
                }
                else
                {
                    XPathInput.Foreground = Brushes.Black;
                }
            }
            catch
            {
                XPathInput.Foreground = Brushes.Red;
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

            SelectionCount.Text = string.Format("{0} element{1} selected",
                HighlightedElements.Count,
                HighlightedElements.Count > 1 ? "s" : String.Empty);

            TextElement first = list[0];
            int index = HighlightedElements.IndexOf(first);
            Select(index);
        }

        #region selection among highlighted elements 

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
            }
        }

        private void Select(int index)
        {
            ClearSelection();
            if ((index >= 0) && (index < HighlightedElements.Count - 1))
            {
                SelectedElement = HighlightedElements[index];
                SelectedElement.Background = Brushes.Yellow;
                SelectedElement.BringIntoView();
            }
        }

        private void NextSelection_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PreviousSelection_Click(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        /*
        private void AddToSelection(IList<TextElement> list)
        {
            if( (list==null) || (list.Count<1))
            {
                return;
            }

            selectedTextElements.AddRange(list);
            foreach(TextElement te in list)
            {                
                Select(te);
            }

            TextElement first = list[0];
            selectedIndex = selectedTextElements.IndexOf(first);
            GoToSelectedIndex();

            SelectionCount.Text = string.Format("{0} element{1} selected",
                selectedTextElements.Count,
                selectedTextElements.Count>1?"s":String.Empty);
        }

        private void Unselect(TextElement te)
        {
            te.Background = null;
        }

        private void Select(TextElement te)
        {
            te.Background = Brushes.Yellow;
        }

        private void NextSelection_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex++;
            if(selectedIndex >= selectedTextElements.Count)
                selectedIndex = selectedTextElements.Count - 1;
            GoToSelectedIndex();
        }

        private void PreviousSelection_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex--;
            if( selectedIndex<0 )
                selectedIndex = 0;
            GoToSelectedIndex();
        }

        private void GoToSelectedIndex()
        {
            NextSelection.IsEnabled = HasNextSelectedIndex;
            PreviousSelection.IsEnabled = HasPreviousSelectedIndex;
            selectedTextElements[selectedIndex].BringIntoView();
        }

        private bool HasNextSelectedIndex
        {
            get
            {
                return (selectedIndex + 1) < selectedTextElements.Count;
            }
        }

        private bool HasPreviousSelectedIndex
        {
            get
            {
                return selectedIndex > 0;
            }
        }
        */

        #endregion



    }
}
