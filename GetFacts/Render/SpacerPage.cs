using GetFacts.Facts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GetFacts.Render
{
    public class SpacerPage:PageStructure
    {
        private Grid articlesGrid;
        private ArticleDisplay _pageDisplay;
        private ArticleDisplay _sectionDisplay;

        public SpacerPage(Facts.Page p, Facts.Section s) : base( new GetFactsHeader() )
        {

            articlesGrid = new Grid()
            {
                Margin = new System.Windows.Thickness(5)
            };

            articlesGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star)
            });

            articlesGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new System.Windows.GridLength(2, System.Windows.GridUnitType.Star)
            });

            _pageDisplay = new ArticleDisplay(false, 0, false);
            Host(_pageDisplay);
            _pageDisplay.Update(p);
            articlesGrid.Children.Add(_pageDisplay);
            Grid.SetRow(_pageDisplay, 0);

            _sectionDisplay = new ArticleDisplay(false, 0, false);
            Host(_sectionDisplay);
            _sectionDisplay.Update(s);
            articlesGrid.Children.Add(_sectionDisplay);
            Grid.SetRow(_sectionDisplay, 1);

            base.Embedded = articlesGrid;
            Initialized += SpacerPage_Initialized;
            Loaded += SpacerPage_Loaded;
        }

        private void SpacerPage_Initialized(object sender, EventArgs e)
        {
            articlesGrid.RowDefinitions[1].Height = new GridLength(0);
        }

        private void SpacerPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            /*if(sectionDisplay!=null)
            {
                GridLengthAnimation gla = new GridLengthAnimation()
                {
                    From = new GridLength(0, GridUnitType.Star),
                    To = new GridLength(2, GridUnitType.Star),
                    Duration = new TimeSpan(0, 0, 1),
                    BeginTime = new TimeSpan(0, 0, 1)
                };
                articlesGrid.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);                            
            }
            else
            {
                
            }*/
            articlesGrid.RowDefinitions[1].Height = new GridLength(0);
        }
    }
}
