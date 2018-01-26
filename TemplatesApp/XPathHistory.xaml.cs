using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TemplatesApp
{
    /// <summary>
    /// Logique d'interaction pour XPathHistory.xaml
    /// </summary>
    public partial class XPathHistory : UserControl
    {
        private readonly ObservableCollection<AbstractXPathBuilder> History = new ObservableCollection<AbstractXPathBuilder>();

        /// <summary>
        /// Evènement qui est généré lorsqu'on double cliquer sur l'un
        /// des éléments de la liste.
        /// </summary>
        public event EventHandler<AbstractXPathBuilder> XPathEntryDoubleClick;

        /// <summary>
        /// Evènement qui est déclenché quand un expression a été
        /// synthétisée à partir de toutes les expressions présentes
        /// dans la liste
        /// </summary>
        public event EventHandler<AbstractXPathBuilder> XPathSolutionClick;

        public XPathHistory()
        {
            InitializeComponent();
            this.DataContext = History;
        }

        public void Add(AbstractXPathBuilder xpath)
        {
            if (History.Contains(xpath) == false)
            {
                History.Insert(0, xpath);
            }
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            History.Clear();
        }

        private void HistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AbstractXPathBuilder xp = HistoryListBox.SelectedItem as AbstractXPathBuilder;
            XPathEntryDoubleClick?.Invoke(this, xp);
        }


        #region drag'n'drop

        Point startPoint;
        AbstractXPathBuilder dragSource = null;

        private void HistoryListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);

            // Get the dragged item
            FrameworkElement tb = (FrameworkElement)e.OriginalSource;
            dragSource = tb.DataContext as AbstractXPathBuilder;
        }

        private void HistoryListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            if( e.LeftButton==MouseButtonState.Pressed)
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;

                if (
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {

                    ListView listView = sender as ListView;

                    // Get the dragged item
                    //FrameworkElement tb = (FrameworkElement)e.OriginalSource;
                    //AbstractXPathBuilder xp = tb.DataContext as AbstractXPathBuilder;
                    if (dragSource == null)
                        return;
                    //ListViewItem listViewItem =
                    //    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                    // Find the data behind the ListViewItem
                    //AbstractXPathBuilder contact = (AbstractXPathBuilder)listView.ItemContainerGenerator.
                    //    ItemFromContainer(listViewItem);

                    // Initialize the drag & drop operation
                    DataObject dragData = new DataObject("AbstractXPathBuilder", dragSource);
                    DragDropEffects result = DragDrop.DoDragDrop(this, dragData, DragDropEffects.Copy);
                }

            }
            else
            {

            }

            
        }


        // Helper to search up the VisualTree
        /*private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }*/


        #endregion

        private void RemoveEntryButton_Click(object sender, RoutedEventArgs e)
        {
            AbstractXPathBuilder xp = HistoryListBox.SelectedItem as AbstractXPathBuilder;
            if (xp != null)
                History.Remove(xp);
        }

        private void TrySumUp_Click(object sender, RoutedEventArgs e)
        {
            AbstractXPathBuilder xp = AbstractXPathBuilder.SynthetizeAndOptimize(History);
            if (xp != null)
                XPathSolutionClick?.Invoke(this, xp);
        }
    }
}
