using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GetFacts.Render
{
    /// <summary>
    /// Cette classe est une simple extension de System.Windows.Controls.Grid,
    /// qui implémente IHostsInformation.
    /// </summary>
    class ArticlesGrid : Grid, IHostsInformation
    {
        /// <summary>
        /// Retourne true si au moins un des contrôles
        /// hébergés dans la grille est un ArticleDisplay 
        /// dont la propriété HasNewInformation vaut true.
        /// Retourne false dans les autres cas.
        /// </summary>
        public bool HasNewInformation
        {
            get
            {
                foreach(ArticleDisplay ad in Children)
                {
                    if (ad.HasNewInformation) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Retourne un texte qui résume le contenu des articles.
        /// </summary>
        public string InformationHeadline
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (ArticleDisplay ad in Children)
                {
                    sb.AppendLine(ad.InformationHeadline);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Retourne un résumé du contenu des articles.
        /// </summary>
        public string InformationSummary
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (ArticleDisplay ad in Children)
                {
                    sb.Append("• ").AppendLine(ad.InformationSummary);
                }
                return sb.ToString();
            }
        }

        internal void CreateRow()
        {
            RowDefinition def = new RowDefinition()
            {
                Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star)
            };
            RowDefinitions.Add(def);
        }
    }
}
