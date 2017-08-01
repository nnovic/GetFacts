﻿using GetFacts.Facts;
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
            ConfigFactory.Save(list);
        }

        private void Restart()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void Root_Initialized(object sender, EventArgs e)
        {
            List <PageConfig> configuration = ConfigFactory.Load();
            foreach(PageConfig config in configuration)
            {
                PageConfigItem pci = new PageConfigItem() { PageConfig = config };
                ListOfConfigItems.Items.Add(pci);
            }
        }

        private void AddPageButton_Click(object sender, RoutedEventArgs e)
        {
            PageConfig pc = new PageConfig();
            PageConfigItem pci = new PageConfigItem() { PageConfig=pc };
            ListOfConfigItems.Items.Add(pci);
            pci.BringIntoView();
            pci.Focus();
        }
    }
}