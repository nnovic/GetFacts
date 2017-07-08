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

namespace TemplatesApp
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

        private void TemplateExplorer_TemplateSelectionChanged(object sender, TemplateExplorer.TemplateSelectionChangedEventArges e)
        {
            SelectTemplateButton.IsEnabled = (e.Path != null);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {            
            ExploreTab.IsEnabled = false;
            EditTab.IsEnabled = false;
            SaveTab.IsEnabled = false;

            SelectTemplateButton.IsEnabled = false;
            CreateTemplateButton.IsEnabled = false;
        }

        private void SelectTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            ExploreTab.IsEnabled = true;
            EditTab.IsEnabled = true;
            SaveTab.IsEnabled = false;
            TabControl.SelectedItem = ExploreTab;
        }
    }
}
