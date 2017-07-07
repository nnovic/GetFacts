using GetFacts.Download;
using System;
using System.Collections.Generic;
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

namespace GetFacts
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            downloadsGrid.ItemsSource = DownloadManager.GetInstance().Items;
        }

        private void AddUrlButton_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string url = InputTextBox.Text;
            Uri uri = new Uri(url, UriKind.Absolute);
            try
            {
                DownloadTask task = DownloadManager.GetInstance().Queue(uri,null);                
            }
            catch(ArgumentException)
            {
            }
            finally
            {
                InputBox.Visibility = Visibility.Collapsed;
            }

        }
    }
}
