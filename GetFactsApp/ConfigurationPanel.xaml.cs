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

namespace GetFacts
{
    /// <summary>
    /// Logique d'interaction pour ConfigurationPanel.xaml
    /// </summary>
    public partial class ConfigurationPanel : UserControl
    {
        public ConfigurationPanel()
        {
            InitializeComponent();
        }


        private void SaveAndRestartButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
            Restart();
        }

        private void Save()
        {
            List<PageConfig> list = new List<PageConfig>();
            foreach(PageConfigItem pci in ListOfConfigItems.Items)
            {
                list.Add(pci.PageConfig);
            }
            ConfigManager.Save(list);
        }

        private void Restart()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void AddPageButton_Click(object sender, RoutedEventArgs e)
        {
            PageConfig pc = new PageConfig();
            PageConfigItem pci = new PageConfigItem() { PageConfig=pc };
            ListOfConfigItems.Items.Add(pci);
            pci.BringIntoView();
            pci.Focus();
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            if (ListOfConfigItems.Items.Count == 0)
            {
                List<PageConfig> configuration = ConfigManager.Load();
                foreach (PageConfig config in configuration)
                {
                    PageConfigItem pci = new PageConfigItem() { PageConfig = config };
                    ListOfConfigItems.Items.Add(pci);
                }

                CacheDirectoryInput.Caption = "Data cache location:";
                CacheDirectoryInput.DefaultDirectory = ConfigManager.GetInstance().DefaultCacheDirectory;
                CacheDirectoryInput.CurrentDirectory = ConfigManager.GetInstance().CacheDirectory;

                TemplatesDirectoryInput.Caption = "Page templates location:";
                TemplatesDirectoryInput.DefaultDirectory = ConfigManager.GetInstance().DefaultTemplatesDirectory;
                TemplatesDirectoryInput.CurrentDirectory = ConfigManager.GetInstance().TemplatesDirectory;

                ConfigFileInput.Caption = "Personal info location:";
                ConfigFileInput.FileMode = true;
                ConfigFileInput.DefaultDirectory = ConfigManager.GetInstance().DefaultConfigFile;
                ConfigFileInput.CurrentDirectory = ConfigManager.GetInstance().ConfigFile;
            }
        }

        private void DoMemorizeLayout()
        {
            WindowPosition wp = WindowPosition.CreateFrom(this);
            ConfigManager.GetInstance().WindowPosition = wp;
        }

        private void MemorizeLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            DoMemorizeLayout();
        }

        private void OnTopButton_Click(object sender, RoutedEventArgs e)
        {
            bool alwaysOnTop = OnTopButton.IsChecked.Value;
            Window w = Window.GetWindow(this);
            w.Topmost = alwaysOnTop;
            DoMemorizeLayout();
        }

        private void OnTopButton_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            OnTopButton.IsChecked = w?.Topmost;
        }
    }
}
