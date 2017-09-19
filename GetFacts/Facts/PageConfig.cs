using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Facts
{
    public class PageConfig
    {
        public PageConfig()
        {
            Enabled = true;
        }

        public string Name
        {
            get; set;
        }

        public string Url
        {
            get; set;
        }

        public string Template
        {
            get; set;
        }

        #region Refresh

        private int refresh = DefaultRefresh;

        /// <summary>
        /// La plus petite période de rafraîchissement que
        /// l'utilisateur peut configurer est 10 minutes
        /// (valeur exprimée en minutes).
        /// </summary>
        /// <see cref="Refresh"/>
        public const int MinRefresh = 10;

        /// <summary>
        /// La plus grande période de rafraîchissement
        /// que l'utilisateur peut configurer est 4 heures
        /// (valeur exprimée en minutes, soit 240).
        /// </summary>
        /// <see cref="Refresh"/>
        public const int MaxRefresh = 4 * 60;

        /// <summary>
        /// valeur initialie du rafraîchissement (valeur
        /// exprimée en minutes).
        /// </summary>
        /// <see cref="Refresh"/>
        public const int DefaultRefresh = 60;

        /// <summary>
        /// Définit, en minutes, l'intervalle de temps entre deux mises à jour
        /// de la page. Cette valeur doit être comprise
        /// entre MinRefresh et MaxRefresh.
        /// </summary>
        /// <see cref="MinRefresh"/>
        /// <see cref="MaxRefresh"/>
        [DefaultValue(60)]
        public int Refresh
        {
            get
            {
                if (refresh < MinRefresh) return DefaultRefresh;
                if (refresh > MaxRefresh) return DefaultRefresh;
                return refresh;
            }
            set
            {
                if( (value<MinRefresh) || (value>MaxRefresh) )
                {
                    throw new ArgumentOutOfRangeException("Refresh");
                }
                else
                {
                    refresh = value;
                }
            }
        }

        #endregion

        [DefaultValue(true)]
        public bool Enabled
        {
            get; set;
        }
    }
}
