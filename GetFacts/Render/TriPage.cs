using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GetFacts.Render
{
    public class TriPage:PageStructure
    {
        private Grid articlesGrid;

        public TriPage(Facts.Page p, Facts.Section s) : base(p)
        {
            articlesGrid = new Grid()
            {
                Margin = new System.Windows.Thickness(5)
            };
            base.Embedded = articlesGrid;
        }


        public void AddArticle(Facts.AbstractInfo ai)
        {
            int rowCount = articlesGrid.RowDefinitions.Count;

            ArticleDisplay ad = new ArticleDisplay(true, rowCount, true);
            Host(ad);

            ad.Update(ai);
            articlesGrid.Children.Add(ad);
            int rowNum = articlesGrid.RowDefinitions.Count;
            Grid.SetRow(ad, rowNum);

            RowDefinition def = new RowDefinition()
            {
                Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star)
            };

            articlesGrid.RowDefinitions.Add(def);
        }

    }
}
