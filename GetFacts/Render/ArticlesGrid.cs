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
    }
}
