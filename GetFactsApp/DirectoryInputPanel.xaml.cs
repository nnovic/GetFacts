using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Logique d'interaction pour DirectoryInputPanel.xaml
    /// </summary>
    public partial class DirectoryInputPanel : UserControl
    {
        private bool fileMode = false;

        public DirectoryInputPanel()
        {
            InitializeComponent();
        }

        [DefaultValue(false)]
        public bool FileMode
        {
            get { return fileMode; }
            set { fileMode = value; OpenButton.IsEnabled = !fileMode; }
        }

        public string Caption
        {
            get { return LabelForCaption.Content as string; }
            set { LabelForCaption.Content = value; }
        }

        public string DefaultDirectory
        {
            get;set;
        }

        public string CurrentDirectory
        {
            get { return FullPath.Text; }
            set { FullPath.Text = value; }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            string path = FullPath.Text;
            Process.Start(path);
        }

        private void Resetbutton_Click(object sender, RoutedEventArgs e)
        {
            CurrentDirectory = DefaultDirectory;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog())
            {
                dlg.Title = Caption;
                dlg.IsFolderPicker = !FileMode;
                dlg.InitialDirectory = CurrentDirectory;

                dlg.AddToMostRecentlyUsedList = false;
                dlg.AllowNonFileSystemItems = false;
                dlg.DefaultDirectory = DefaultDirectory;
                dlg.EnsureValidNames = true;
                dlg.Multiselect = false;
                dlg.ShowPlacesList = true;

                if (FileMode)
                {
                    CommonFileDialogFilter filter = new CommonFileDialogFilter(".json", ".json");
                    dlg.Filters.Add(filter);
                }

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    CurrentDirectory = dlg.FileName;
                }
            }
        }
    }
}
