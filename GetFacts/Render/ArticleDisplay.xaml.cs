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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GetFacts.Render
{
    /// <summary>
    /// Logique d'interaction pour ArticleDisplay.xaml
    /// </summary>
    public partial class ArticleDisplay : UserControl, ICanDock
    {
        private readonly bool enableAnimations;
        private readonly int orderOfAppearance;
        private string browserUrl = null;
        const double textWidthRatio = 0.5;

        public ArticleDisplay(): this(false, 0, false)
        {

        }

        public ArticleDisplay(bool enableAnimations, int orderOfAppearance, bool enableHighlight)
        {
            this.enableAnimations = enableAnimations;
            this.orderOfAppearance = orderOfAppearance;

            InitializeComponent();

            if(enableHighlight)
            {
                MouseEnter += UserControl_MouseEnter;
                MouseLeave += UserControl_MouseLeave;
            }

            DoLayout(new Size(ActualWidth, ActualHeight));
        }

        internal void Update(Facts.AbstractInfo info)
        {
            articleTitle.Text = info.Title;
            articleText.Text = info.Text;
            browserUrl = info.BrowserUrl;

            if (string.IsNullOrEmpty(info.IconUrl) == false)
            {
                iconTask = DownloadManager.GetInstance().FindOrQueue(info.IconUri);
                iconTask.TaskFinished += IconTask_TaskFinished;
                iconTask.PropertyChanged += IconTask_PropertyChanged;
                iconTask.TriggerIfTaskFinished();
            }

            if( string.IsNullOrEmpty(browserUrl)==false)
            {
                textContainer.Cursor = Cursors.Hand;
            }
        }

        private void IconTask_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if( e.PropertyName=="Progress")
            {
                int progress = iconTask.Progress;
                mediaDisplay.ShowProgress(progress);
            }
        }


        #region downloads

        private DownloadTask iconTask;

        private void IconTask_TaskFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                mediaDisplay.ShowImage(iconTask.LocalFile, articleTitle.Text);
            }), null);
            
        }

        #endregion

        #region layout

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
            if (IsVertical(s))
            {
                DispositionVerticale();
            }
            else if(IsHorizontal(s))
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
            if(iconTask!=null)
            {
                iconTask.TaskFinished -= IconTask_TaskFinished;
                iconTask.PropertyChanged -= IconTask_PropertyChanged;
                iconTask = null;
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            textContainer.RowDefinitions[1].Height = new GridLength(0);
            
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

        private void TextContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(browserUrl) == false)
            {
                Process.Start(browserUrl);
                e.Handled = true;
            }
        }

        #region events

        private void ArticleIcon_MouseEnter(object sender, MouseEventArgs e)
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
            }
        }

        public void Undock(MediaDisplay md)
        {
            imageContainer.Children.Remove(md);
        }


        public void Dock(MediaDisplay md)
        {
            imageContainer.Children.Add(md);
        }

        #endregion

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            bgBorder.Background = Brushes.Gray;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            bgBorder.Background = Brushes.Yellow;
        }
    }

}
