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
    public partial class PageStructure : Page, IFreezable, ICanDock, IHostsInformation
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

        #region IHostsInformation

        /// <summary>
        /// Si Embedded est un objet qui implémente IHostsInformation,
        /// retourne la valeur courante de ((IHostsInformation)Embedded).HasNewInformation.
        /// Sinon, retourne false.
        /// </summary>
        /// <see cref="Embedded"/>
        /// <seealso cref="IHostsInformation"/>
        public bool HasNewInformation
        {
            get
            {
                if( factsBorder.Child is IHostsInformation ihn)
                {
                    return ihn.HasNewInformation;
                }
                return false;
            }
        }

        /// <summary>
        /// Retourne le titre de la page pour résumer les articles
        /// présentés.
        /// </summary>
        public string InformationHeadline
        {
            get { return pageDisplay.InformationHeadline; }
        }

        /// <summary>
        /// Retourne le résumé des articles
        /// présentés.
        /// </summary>
        public string InformationSummary
        {
            get
            {
                if (factsBorder.Child is IHostsInformation ihn)
                {
                    return ihn.InformationSummary;
                }
                return null;
            }
        }

        #endregion 

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

        #region events

        public event EventHandler<FreezeEventArgs> Frozen;
        public event EventHandler<FreezeEventArgs> Unfrozen;

        protected void OnFrozen(CauseOfFreezing cause)
        {
            FreezeEventArgs args = new FreezeEventArgs(cause);            
            Frozen?.Invoke(this, args);
            pauseSymbol.BeginAnimation(Canvas.OpacityProperty, pauseAnimation);
            pauseDisplay.Visibility = Visibility.Visible;
        }

        protected void OnUnfrozen(CauseOfFreezing cause)
        {
            FreezeEventArgs args = new FreezeEventArgs(cause);
            Unfrozen?.Invoke(this, args);
            pauseDisplay.Visibility = Visibility.Hidden;
            pauseSymbol.BeginAnimation(Canvas.OpacityProperty, null);
        }

        private void FactsBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            OnFrozen(CauseOfFreezing.CursorOnArticle);
        }

        private void FactsBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            OnUnfrozen(CauseOfFreezing.CursorOnArticle);
        }

        #endregion

        #region media docking

        MediaDisplay dockedMedia = null;

        public void Undock(MediaDisplay md)
        {
            if(dockedMedia!=md)
            {
                throw new Exception();
            }

            dockedMedia = null;
            mediaDock.Children.Remove(md);            
            mediaGrid.Visibility = Visibility.Hidden;
            OnUnfrozen(CauseOfFreezing.ZoomOnMedia); // restart stopwatch
            Console.WriteLine("Media detached from Page");
        }

        public void Dock(MediaDisplay md)
        {
            if( dockedMedia!=null)
            {
                throw new Exception();
            }

            OnFrozen(CauseOfFreezing.ZoomOnMedia); // disable stop watch
            dockedMedia = md;
            mediaDock.Children.Add(md);

            if (string.IsNullOrEmpty(md.Caption))
            {
                mediaTitle.Visibility = Visibility.Hidden;
            }
            else
            {
                mediaTitle.Visibility = Visibility.Visible;
                mediaTitle.Text = md.Caption;
            }

            mediaGrid.Visibility = Visibility.Visible;
            Console.WriteLine("Media attached to Page");
        }

        #endregion

        private void MediaGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MediaDisplay md = dockedMedia;
            ICanDock target = md.Tag as ICanDock;
            Undock(md);
            target.Dock(md);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Frozen = null;
            Unfrozen = null;
            pauseSymbol.BeginAnimation(Canvas.OpacityProperty, null);
            mediaDock.Children.Clear();
        }
    }
}
