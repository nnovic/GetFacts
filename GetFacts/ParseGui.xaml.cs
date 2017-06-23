using GetFacts.Parse;
using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GetFacts
{
    /// <summary>
    /// Logique d'interaction pour ParseGui.xaml
    /// </summary>
    public partial class ParseGui : Window
    {
        private AbstractParser parser = null;

        public ParseGui()
        {
            InitializeComponent();
        }

        private void StartWait(string reason)
        {
            this.Dispatcher.Invoke(() => {
                PleaseWait.Visibility = Visibility.Visible;
                PleaseWaitCaption.Text = reason;
            });
        }

        private void StopWait()
        {
            this.Dispatcher.Invoke(() => {
                PleaseWait.Visibility = Visibility.Collapsed;
            });
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartWait("Getting page...");

            Action<object> downloadAction = (object arg) =>
            {
                string s = (string)arg;
                s = DownloadHtml(s);
                ParseHtml(s);
            };

            string url = UrlInput.Text;
            Task downloadTask = new Task(downloadAction, url);
            downloadTask.Start();
        }

        private void ParseHtml(string file)
        {
            StartWait("Analyzing page...");

            parser = new HtmlParser();
            parser.Load(file);

            this.Dispatcher.Invoke(()=> 
            {
                CodeSourceView.Document = parser.SourceCode;
                CodeSourceTree.Items.Add(parser.SelectedSource);
            });            

            StopWait();
        }

        private string DownloadHtml(string url)
        {
            string tmpFile = System.IO.Path.GetTempFileName();
            WebClient wc = new WebClient();
            wc.DownloadFile(url, tmpFile);
            return tmpFile;
        }

        private void CodeSourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem oldItem = e.OldValue as TreeViewItem;
            TreeViewItem newItem = e.NewValue as TreeViewItem;

            if( oldItem != null )
            {
                TextElement i = parser.GetTextElementOf(oldItem);
                i.Background = null;
            }
            
            if (newItem == null)
            {
                XPathBox.Text = "(no selection)";
            }
            else
            {

                // Refresh xpath
                string xp = parser.GetXPathOf(newItem);
                XPathBox.Text = xp;

                // Highlight selection
                // and scroll view
                TextElement i = parser.GetTextElementOf(newItem);                
                i.Background = Brushes.Yellow;

                // Display attributes
                AttributesGrid.Items.Clear();
                Hashtable attributes = parser.GetAttributesOf(newItem);
                foreach (string key in attributes.Keys)
                {
                    string value = (string)attributes[key];
                    AttributesGrid.Items.Add(new { Name = key, Value = value });
                }
            }

            
        }
    }
}
