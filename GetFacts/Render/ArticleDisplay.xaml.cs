using GetFacts.Download;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GetFacts.Render
{
    /// <summary>
    /// Logique d'interaction pour ArticleDisplay.xaml
    /// </summary>
    public partial class ArticleDisplay : UserControl, ICanDock, IHostsInformation
    {
        private readonly bool enableAnimations;
        private readonly int orderOfAppearance;
        private string browserUrl = null;
        const double textWidthRatio = 0.5;
        private readonly Brush normalBrush = new SolidColorBrush(Color.FromArgb(0x7F, 0xFF, 0xFF, 0xFF));

        /// <summary>
        /// Mémorise la référence de l'objet qui contient les
        /// données à afficher. Cette référence n'est indispensable
        /// que pour gérer le changement d'état de AbstractInfo.IsNew
        /// en fonction des actions de l'utilisateur.
        /// </summary>
        private Facts.AbstractInfo info = null;

        ~ArticleDisplay()
        {
            // Pour debug uniquement, afin de vérifier que les instances de
            // cette classe sont bien détruits à un moment ou à un autre.
        }

        public ArticleDisplay(): this(false, 0)
        {

        }

        public ArticleDisplay(bool enableAnimations, int orderOfAppearance)
        {
            this.enableAnimations = enableAnimations;
            this.orderOfAppearance = orderOfAppearance;

            InitializeComponent();

            if(enableAnimations)
            {
                MouseEnter += UserControl_MouseEnter;
                MouseLeave += UserControl_MouseLeave;
            }

            DoLayout(new Size(ActualWidth, ActualHeight));
        }

        public void Update(Facts.AbstractInfo info)
        {
            // Mémorise la référence de l'objet qui contient les
            // données à afficher. Cette référence n'est indispensable
            // que pour gérer le changement d'état de AbstractInfo.IsNew
            // en fonction des actions de l'utilisateur.
            this.info = info;

            if(info.HasContent==false)
            {
                articleTitle.Text = "(no content to display)";
                articleText.Text = "O_o";
                return;
            }

            articleTitle.Text = info.Title;
            articleText.Text = info.Text;
            mediaDisplay.Caption = info.Title;

            browserUrl = info.BrowserUri?.AbsoluteUri;

            StartIconTask();

            if( string.IsNullOrEmpty(browserUrl)==false)
            {
                /*textContainer.*/Cursor = Cursors.Hand;
                /*textContainer.*/ToolTip = browserUrl;
            }

            this.HasNewInformation = info.IsNew;

            if(enableAnimations)
            {
                if(this.HasNewInformation)
                {
                    bgBorder.Background = Brushes.Green;
                    ShakeShakeAnimation ssa = new ShakeShakeAnimation(this.Margin)
                    {
                        BeginTime = new TimeSpan(0, 0, 0, 1, 200 * orderOfAppearance)
                    };
                    this.BeginAnimation(FrameworkElement.MarginProperty, ssa);
                }
            }
        }

        #region IHostsInformation
        
        /// <summary>
        /// Retourne 'true' si, lors du dernier appel à Update(),
        /// l'article passé en paramètre avant sa propriété IsNew à true.
        /// Sinon, retourne 'false'. 
        /// </summary>
        /// <remarks>Retournera 'false' systématiquement après un appel à Activate()</remarks>
        /// <see cref="Update(Facts.AbstractInfo)"/>
        /// <see cref="Activate"/>
        public bool HasNewInformation
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le titre de l'article en tant que résumé
        /// de l'information
        /// hébergée dans l'objet qui implémente cette interface.
        /// </summary>
        public string InformationHeadline
        {
            get { return articleTitle.Text; }
        }

        /// <summary>
        /// Retourne le titre de l'article en tant que résumé
        /// de l'information
        /// hébergée dans l'objet qui implémente cette interface.
        /// </summary>
        public string InformationSummary
        {
            get { return articleTitle.Text; }
        }

        #endregion

        #region downloads

        #region icon 

        private DownloadTask iconTask;

        private void StartIconTask()
        {
            if (string.IsNullOrEmpty(info.IconUrl) == false)
            {
                iconTask = DownloadManager.GetInstance().FindOrQueue(info.IconUri,null);
                iconTask.TaskFinished += IconTask_TaskFinished;
                iconTask.PropertyChanged += IconTask_PropertyChanged;
                iconTask.TriggerIfTaskFinished();
            }  
            else
            {
                StartMediaTask();
            }
        }

        private void CleanIconTask()
        {
            if(iconTask != null)
            {
                iconTask.TaskFinished -= IconTask_TaskFinished;
                iconTask.PropertyChanged -= IconTask_PropertyChanged;
                iconTask = null;
            }
        }

        private void IconTask_TaskFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    mediaDisplay.ShowImage(iconTask.LocalFile);
                }
                catch
                {
                }
            }), null);

            StartMediaTask();
        }

        private void IconTask_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Progress")
            {
                int progress = iconTask.Progress;
                mediaDisplay.ShowProgress(progress);
            }
        }

        #endregion

        #region media 

        private DownloadTask mediaTask;

        private void StartMediaTask()
        {
            if (string.IsNullOrEmpty(info.MediaUrl) == false)
            {
                mediaTask = DownloadManager.GetInstance().FindOrQueue(info.MediaUri, null);
                mediaTask.TaskFinished += MediaTask_TaskFinished;
                mediaTask.PropertyChanged += MediaTask_PropertyChanged;
                mediaTask.TriggerIfTaskFinished();
            }
        }

        private void CleanMediaTask()
        {
            if (mediaTask != null)
            {
                mediaTask.TaskFinished -= MediaTask_TaskFinished;
                mediaTask.PropertyChanged -= MediaTask_PropertyChanged;
                mediaTask = null;
            }
        }

        private void MediaTask_TaskFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    mediaDisplay.ShowMedia(mediaTask.LocalFile);
                }
                catch
                {
                }
            }), null);
        }

        private void MediaTask_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Progress")
            {
                int progress = mediaTask.Progress;
                mediaDisplay.ShowProgress(progress);
            }
        }

        #endregion

        #endregion

        #region layout

        public enum Orientation
        {
            Automatic =0, // will be the default value
            Horizontal,
            Vertical,
            Square
        }

        public Orientation DesiredOrientation
        {
            get; set;
        }

        private bool IsVertical(Size s)
        {
            return s.Height >= (1.5* s.Width);
        }

        private bool IsHorizontal(Size s)
        {
            return s.Width >= (1.5 * s.Height);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DoLayout(e.NewSize);
        }

        private void DoLayout(Size s)
        {
            if (IsVertical(s) || (DesiredOrientation==Orientation.Vertical) )
            {
                DispositionVerticale();
            }
            else if(IsHorizontal(s) || (DesiredOrientation==Orientation.Horizontal) )
            {
                DispositionHorizontale();
            }
            else
            {
                DispositionCarre();
            }
        }

        private void DispositionVerticale()
        {
            rotatingGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Star);
            rotatingGrid.ColumnDefinitions[1].Width = new GridLength(2, GridUnitType.Star);
            Grid.SetRow(textContainer, 0);
            Grid.SetColumn(textContainer, 1);

            rotatingGrid.LayoutTransform = new RotateTransform(90);
            mediaDisplay.LayoutTransform = new RotateTransform(-90);
            textContainer.LayoutTransform = new RotateTransform(180);

            articleTitle.MaxWidth = ActualWidth * textWidthRatio;
            articleText.MaxWidth = ActualWidth * textWidthRatio;
        }

        private void DispositionHorizontale()
        {
            rotatingGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Star);
            rotatingGrid.ColumnDefinitions[1].Width = new GridLength(2, GridUnitType.Star);
            Grid.SetRow(textContainer, 0);
            Grid.SetColumn(textContainer, 1);

            rotatingGrid.LayoutTransform = Transform.Identity;
            mediaDisplay.LayoutTransform = Transform.Identity;
            textContainer.LayoutTransform = Transform.Identity;

            articleTitle.MaxWidth = ActualWidth * textWidthRatio;
            articleText.MaxWidth = ActualWidth * textWidthRatio;
        }

        private void DispositionCarre()
        {
            rotatingGrid.RowDefinitions[1].Height = new GridLength(2, GridUnitType.Star);
            rotatingGrid.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Star);
            Grid.SetRow(textContainer, 1);
            Grid.SetColumn(textContainer, 0);

            rotatingGrid.LayoutTransform = Transform.Identity;
            mediaDisplay.LayoutTransform = Transform.Identity;
            textContainer.LayoutTransform = Transform.Identity;

            articleTitle.MaxWidth = ActualWidth * textWidthRatio;
            articleText.MaxWidth = ActualWidth * textWidthRatio;
        }

        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanIconTask();
            CleanMediaTask();
            imageContainer.MouseLeftButtonUp -= ImageContainer_MouseLeftButtonUp;

            // Arrêter la lecture du média actuellement verrouillé par
            // MediaDisplay; indispensable pour assurer la libération
            // des ressources par le GarbageCollector.
            mediaDisplay.Dispose(); 
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            textContainer.RowDefinitions[1].Height = new GridLength(0);

            if (enableAnimations)
            {
                imageContainer.Cursor = Cursors.Hand;
                imageContainer.ToolTip = "Click to zoom";
                imageContainer.MouseLeftButtonUp += ImageContainer_MouseLeftButtonUp;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(articleText.Text) == false)
            {
                if (enableAnimations)
                {
                    GridLengthAnimation gla = new GridLengthAnimation()
                    {
                        From = new GridLength(0, GridUnitType.Star),
                        To = new GridLength(2, GridUnitType.Star),
                        Duration = new TimeSpan(0, 0, 0, 0, 500),
                        BeginTime = new TimeSpan(0, 0, 0, 2, 200 * orderOfAppearance)
                    };
                    textContainer.RowDefinitions[1].BeginAnimation(
                        RowDefinition.HeightProperty, gla);                    
                }
                else
                {
                    textContainer.RowDefinitions[1].Height = new GridLength(2, GridUnitType.Star);
                }
            }
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            // Lorsque l'utilisateur clique sur ce contrôle, alors les données qu'il affiche
            // deviennent "vieille" (si c'est le comportement choisi par l'utilisateur!).
            if ((this.info != null)
                && (this.info.IsNewBehavior == Facts.AbstractInfo.IsNewPropertyGets.OldOnMouseClick))
            {
                this.info.IsNew = false;
            }

            if (string.IsNullOrEmpty(browserUrl) == false)
            {
                Process.Start(browserUrl);
                e.Handled = true;
            }
        }

        #region events

        public void Undock(MediaDisplay md)
        {
            imageContainer.MouseLeftButtonUp -= ImageContainer_MouseLeftButtonUp;
            imageContainer.Children.Remove(md);
            md.Tag = this;
            Console.WriteLine("Media detached from ArticleDisplay");
        }


        public void Dock(MediaDisplay md)
        {
            imageContainer.Children.Add(md);
            imageContainer.MouseLeftButtonUp += ImageContainer_MouseLeftButtonUp;
            Console.WriteLine("Media attached to ArticleDisplay");
        }

        private void ImageContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement current = this.Parent as FrameworkElement;
            ICanDock target = null;

            while(  current !=null )
            {
                target = current as ICanDock;
                if (target != null) break;
                current = current.Parent as FrameworkElement;
            }

            if(target!=null)
            {
                Undock(mediaDisplay);
                target.Dock(mediaDisplay);
                OnMouseLeave(null);
                e.Handled = true;
            }
        }

        #endregion

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Deactivate();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Activate();
        }


        private void Deactivate()
        {
            Console.WriteLine("ArticleDisplay.Deactivate");
            bgBorder.Background = normalBrush;
            mediaDisplay.SmoothVideo = false;

        }

        private void Activate()
        {
            Console.WriteLine("ArticleDisplay.Activate");

            bgBorder.Background = Brushes.Yellow;
            this.BeginAnimation(FrameworkElement.MarginProperty, null);
            this.HasNewInformation = false;

            // Lorsque le curseur de la souris survole ce contrôle, alors les données qu'il affiche
            // deviennent "vieille" (si c'est le comportement choisi par l'utilisateur!).
            if ((this.info!=null) 
                && (this.info.IsNewBehavior== Facts.AbstractInfo.IsNewPropertyGets.OldOnMouseHover) )
            {
                this.info.IsNew = false;
            }

            mediaDisplay.SmoothVideo = true;
        }
    }

}
