using System;
using System.Collections.Generic;
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
        public DirectoryInputPanel()
        {
            InitializeComponent();
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
    }
}
