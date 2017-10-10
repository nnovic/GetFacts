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
            get; set;
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
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
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

        private bool smoothVideo = false;

        /// <summary>
        /// Définit si la vidéo en cours d'affichage doit être
        /// jouée de façon "fluide" (lecture normale) ou "hachée"
        /// (sauts d'image successifs d'1 seconde).
        /// </summary>
        public bool SmoothVideo
        {
            get
            {
                return smoothVideo;
            }
            set
            {
                smoothVideo = value;
                if(smoothVideo)
                {
                    articleMedia.Play();
                }
                else
                {
                    articleMedia.Pause();
                }
            }
        }

        /// <summary>
        /// Toutes les secondes, mettre à jour la barre de progression
        /// de lecture du média en cours. S'il s'agit d'une vidéo et
        /// que SmoothVideo vaut "false", alors avance la position
        /// de la vidéo d'1 seconde.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (articleMedia.NaturalDuration.HasTimeSpan)
            {
                double maxDuration = articleMedia.NaturalDuration.TimeSpan.TotalSeconds;
                double currentPosition = articleMedia.Position.TotalSeconds;

                if (articleMedia.HasVideo && (SmoothVideo == false))
                {
                    currentPosition = Math.Min(currentPosition + 1.0, maxDuration);
                    articleMedia.Position = TimeSpan.FromSeconds(currentPosition);
                }

                mediaProgressValue.Maximum = maxDuration;
                mediaProgressValue.Value = currentPosition;
                mediaProgressContainer.Visibility = Visibility.Visible;
            }
            else
            {
                mediaProgressValue.Value = 0;
                mediaProgressContainer.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// une fois le contrôle chargé, démarrer la lecture
        /// du média.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArticleMedia_Loaded(object sender, RoutedEventArgs e)
        {
            articleMedia.Play();
        }

        private void ArticleMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Lorsque le fichier média est ouvert,
            // mettre la lecture en pause immédiatement.

            articleMedia.Pause();
            //mediaProgressValue.Value = 0;
            //mediaProgressContainer.Visibility = Visibility.Visible;            
            dispatcherTimer.Start();
        }

        private void ArticleMedia_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            articleMedia.Stop();
            mediaProgressContainer.Visibility = Visibility.Hidden;
        }

        private void ArticleMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            mediaProgressContainer.Visibility = Visibility.Hidden;
        }


        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Permet de libérer les ressources "verrouillées" par
        /// ce contrôle (image, vidéo, etc...), ce qui permettra 
        /// ensuite à cet objet d'être détruit par le garbage 
        /// collector.
        /// </summary>
        internal void Dispose()
        {
            dispatcherTimer.Stop();
            articleIcon.Source = null;
            articleMedia.Source = null;
        }
    }
}
