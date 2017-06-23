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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GetFacts.Render
{
    /// <summary>
    /// Logique d'interaction pour PagePage.xaml
    /// </summary>
    public partial class PageStructure : Page, IFreezable
    {
        DoubleAnimation pauseAnimation;
        
        public PageStructure(Facts.AbstractInfo info)
        {
            InitializeComponent();
            pauseAnimation = new DoubleAnimation()
            {
                From=0, 
                To=1,
                Duration=new Duration(new TimeSpan(0,0,1)),
                RepeatBehavior= RepeatBehavior.Forever,
                AutoReverse=true
            };
            pageDisplay.Update(info);
            DoLayout(new Size(ActualWidth,ActualHeight));
        }
        
        public string PauseOpactiy { get { return "0.65"; } }

        internal UIElement Embedded
        {
            set { factsBorder.Child = value; }
            get { return factsBorder.Child; }
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            UnHost();
        }

        #region layout

        private bool IsVertical(Size s)
        {
            return s.Height >s.Width;
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
            else
            {
                DispositionHorizontale();
            }
        }

        private void DispositionVerticale()
        {
            // pageDisplay
            Grid.SetRowSpan(pageDisplay, 1);
            Grid.SetColumnSpan(pageDisplay, 2);

            Grid.SetRowSpan(pauseDisplay, 1);
            Grid.SetColumnSpan(pauseDisplay, 2);


            // factsContainer
            Grid.SetRow(factsContainer, 1);
            Grid.SetRowSpan(factsContainer, 1);
            Grid.SetColumn(factsContainer, 0);
            Grid.SetColumnSpan(factsContainer, 2);
        }

        private void DispositionHorizontale()
        {
            // pageDisplay
            Grid.SetRowSpan(pageDisplay, 2);
            Grid.SetColumnSpan(pageDisplay, 1);

            Grid.SetRowSpan(pauseDisplay, 2);
            Grid.SetColumnSpan(pauseDisplay, 1);

            // factsContainer
            Grid.SetRow(factsContainer, 0);
            Grid.SetRowSpan(factsContainer, 2);
            Grid.SetColumn(factsContainer, 1);
            Grid.SetColumnSpan(factsContainer, 1);
        }

        #endregion

        #region articles hosting

        private readonly List<ArticleDisplay> hostedArticles = new List<ArticleDisplay>();

        private void UnHost()
        {
            foreach(ArticleDisplay ad in hostedArticles)
            {
                ad.MediaTriggered -= Ad_MediaTriggered;
            }
        }

        protected void Host(ArticleDisplay ad)
        {
            if (hostedArticles.Contains(ad))
                return;

            hostedArticles.Add(ad);
            ad.MediaTriggered += Ad_MediaTriggered;
        }

        private void Ad_MediaTriggered(object sender, ArticleDisplay.MediaEventArgs e)
        {
            OnFrozen();
            //mediaGrid.Visibility = Visibility.Visible;
            //mediaPlayer.Source = e.Media;
        }

        #endregion

        #region events

        public event EventHandler<FreezeEventArgs> Frozen;
        public event EventHandler<FreezeEventArgs> Unfrozen;

        protected void OnFrozen()
        {
            FreezeEventArgs args = new FreezeEventArgs();            
            Frozen?.Invoke(this, args);
            pauseSymbol.BeginAnimation(Canvas.OpacityProperty, pauseAnimation);
            pauseDisplay.Visibility = Visibility.Visible;
        }

        protected void OnUnfrozen()
        {
            FreezeEventArgs args = new FreezeEventArgs();
            Unfrozen?.Invoke(this, args);
            pauseDisplay.Visibility = Visibility.Hidden;
            pauseSymbol.BeginAnimation(Canvas.OpacityProperty, null);
        }

        private void FactsBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            OnFrozen();
        }

        private void FactsBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            OnUnfrozen();
        }

        #endregion
    }
}
