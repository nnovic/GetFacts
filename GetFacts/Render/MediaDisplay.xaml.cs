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

namespace GetFacts.Render
{
    /// <summary>
    /// Logique d'interaction pour MediaDisplay.xaml
    /// </summary>
    public partial class MediaDisplay : UserControl
    {
        public MediaDisplay()
        {
            InitializeComponent();
        }

        public string Caption
        {
            get;set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress">0-100</param>
        public void ShowProgress(int progress)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                progressContainer.Visibility = Visibility.Visible;
                progressValue.Text = string.Format("{0} %", progress);
            }));
        }

        public void ShowImage(string file)
        {
            try
            {
                BitmapImage bmpI = new BitmapImage();
                bmpI.BeginInit();
                bmpI.UriSource = new Uri(file, UriKind.RelativeOrAbsolute);
                bmpI.EndInit();
                articleIcon.Source = bmpI;
                progressContainer.Visibility = Visibility.Hidden;
            }
            // Since the action is executed asynchronously, the Dispatcher
            // might execute the above code at a time when iconTask has
            // already become invalid. Just ignore all errors.
            catch { }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            progressContainer.Visibility = Visibility.Hidden;
        }
    }
}
