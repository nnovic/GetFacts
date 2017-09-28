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
using System.Windows.Threading;

namespace GetFacts.Render
{
    /// <summary>
    /// Logique d'interaction pour MediaDisplay.xaml
    /// </summary>
    public partial class MediaDisplay : UserControl
    {
        private readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();

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
                downloadProgressContainer.Visibility = Visibility.Visible;
                downloadProgressValue.Text = string.Format("{0} %", progress);
            }));
        }

        public void ShowMedia(string file)
        {
            try
            {
                articleMedia.Visibility = Visibility.Visible;
                articleMedia.Source = new Uri(file, UriKind.Absolute);
                downloadProgressContainer.Visibility = Visibility.Hidden;
            }
            // Since the action is executed asynchronously, the Dispatcher
            // might execute the above code at a time when mediaTask has
            // already become invalid. Just ignore all errors.
            catch { }
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
                downloadProgressContainer.Visibility = Visibility.Hidden;
            }
            // Since the action is executed asynchronously, the Dispatcher
            // might execute the above code at a time when iconTask has
            // already become invalid. Just ignore all errors.
            catch { }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            downloadProgressContainer.Visibility = Visibility.Hidden;
            mediaProgressContainer.Visibility = Visibility.Hidden;
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            articleMedia.IsMuted = false;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            articleMedia.IsMuted = true;
        }

        #region timer management

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = articleMedia.Position;
            double secondes = ts.TotalSeconds;
            mediaProgressValue.Value = secondes;
            mediaProgressContainer.Visibility = Visibility.Visible;
        }

        private void ArticleMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            Duration d = articleMedia.NaturalDuration;
            TimeSpan ts = d.TimeSpan;
            double secondes = ts.TotalSeconds;
            mediaProgressValue.Maximum = secondes;
            mediaProgressValue.Value = 0;
            dispatcherTimer.Start();
        }

        private void ArticleMedia_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }

        private void ArticleMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            mediaProgressContainer.Visibility = Visibility.Hidden;
        }
        #endregion


    }
}
