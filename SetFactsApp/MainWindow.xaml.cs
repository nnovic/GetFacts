using GetFacts;
using GetFacts.Facts;
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

namespace SetFactsApp
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = ConfigFactory.GetInstance().ConfigFile;
            ConfigFileSelection.Items.Add(path);
            ConfigFileSelection.SelectedItem = path;
        }

        private void ConfigFileSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string path = ConfigFileSelection.SelectedItem as string;
            LoadConfig(path);
        }

        private void LoadConfig(string path)
        {
            List<PageConfig> config = ConfigFactory.GetInstance().CreateConfig(path);
            foreach(PageConfig pc in config)
            {
                PageConfigListItem item = new PageConfigListItem() { PageConfig=pc };
                ConfigList.Items.Add(item);
            }
        }
    }
}
