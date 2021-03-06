﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace GetFacts.Render
{
    public class TriPage:PageStructure,ICustomPause
    {
        private ArticlesGrid articlesGrid;
        private int articlesCount;

        ~TriPage()
        {
            // Pour debug uniquement, afin de vérifier que les instances de
            // cette classe sont bien détruits à un moment ou à un autre.
        }

        public TriPage(Facts.Page p, Facts.Section s) : base(p)
        {
            articlesGrid = new ArticlesGrid()
            {
                //Margin = new System.Windows.Thickness(5)
            };
            articlesGrid.CreateRow();
            articlesGrid.CreateRow();
            articlesGrid.CreateRow();
            articlesCount = 0;

            base.Embedded = articlesGrid;

            
            // Note: the follwing is for debugging purpose only,
            // as no content from a disabled page should
            // ever be displayed !
            if(!p.Enabled)
            {
                Background = Brushes.Gray;
            }
        }

        #region ICustomPause

        public int MaxPageDisplayDuration
        {
            get { return 60; }
        }

        public int MinPageDisplayDuration
        {
            get { return 10; }
        }

        #endregion

        public void AddArticle(Facts.AbstractInfo ai)
        {
            if(articlesCount>=3)
            {
                throw new Exception();
            }

            ArticleDisplay ad = new ArticleDisplay(true, articlesCount);
            ad.Update(ai);
            articlesGrid.Children.Add(ad);
            //int rowNum = articlesGrid.RowDefinitions.Count;
            Grid.SetRow(ad, articlesCount);
            articlesCount++;
        }

    }
}
