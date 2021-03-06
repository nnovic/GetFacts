﻿using BusinessLogic.Behaviour.Other.SecuritiesSearch;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Facts
{   
    public class Article:AbstractInfo
    {
        /// <summary>
        /// Destruction de cet objet:
        /// - s'assurer que toutes les notifications poussées dans
        ///   NotificationSystem par cet objet soient supprimées.
        /// </summary>
        ~Article()
        {
            NotificationSystem.GetInstance().RemoveAll(this);
        }

        /// <summary>
        /// Permet de transférer le contenu de l'article en paramètre
        /// dans cette instance. Ce mécanisme est utilisé par 
        /// Section#AddOrUpdateArticle pour mettre à jour
        /// un article existant.
        /// </summary>
        /// <param name="source"></param>
        internal void Update(Article source)
        {
            UpdateInfo(source);            
        }

        /// <summary>
        /// Initialise le contenu de cet article avec les
        /// infos provenant d'un XPathNavigator, en suivant
        /// les instructions d'un ArticleTemplate.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="template"></param>
        /// <remarks>Bloque toutes les exceptions qui pourraient survenir durant
        /// l'exécution de cette méthode.</remarks>
        /// <seealso cref="NotificationKeys.ArticleUpdateError"/>
        internal void Update(XPathNavigator nav, ArticleTemplate template)
        {

            var notification = new NotificationSystem.Notification(this,
                (int)NotificationKeys.ArticleUpdateError)
            {
                Title = Identifier,
                Description = "Article update error."
            };

            try
            {
                UpdateInfo(nav, template);
                NotificationSystem.GetInstance().Remove(notification);
            }
            catch(Exception e)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(notification.Description);
                sb.AppendFormat("{0} occured while parsing data from {1}.",
                    e.GetType().Name, this.BaseUri.ToString()).AppendLine();
                sb.AppendFormat("The original error message is: \"{0}\"", e.Message);
                notification.Description = sb.ToString();
                NotificationSystem.GetInstance().Add(notification);
            }
        }

        #region évaluer la ressemblance avec un autre article 

        private static double CalculateResemblance(string s1, string s2, double cutoff)
        {
            double output;

            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            {
                output = -1;
            }
            else
            {
                double score = JaroWinkler.RateSimilarity(s1, s2);
                if (score >= cutoff)
                    output = score;
                else
                    output = 0;
            }

            return output;
        }
        

        private static double CalculateResemblance(Article a1, Article a2)
        {
            double articleScore = 0;
            double scoreDivider = 0;

            const double titleCutoff = 0.9;
            double textCutOff;

            double titleScore = CalculateResemblance(a1.Title, a2.Title, titleCutoff);
            if(titleScore>=0)
            {
                articleScore += titleScore;
                textCutOff = 0.7;
                scoreDivider++;
            }
            else
            {
                // le titre n'a pas pu être évalué,
                // accorder une importance accrue à la comparaison du texte.
                textCutOff = 0.9;
            }

            double textScore = CalculateResemblance(a1.Text, a2.Text, textCutOff);
            if( textScore>=0 )
            {
                articleScore += textScore;
                scoreDivider++;
            }
            else
            {

            }


            if (scoreDivider <= 0) return 0;
            return articleScore / scoreDivider;
            
            

        }

        internal double CalculateResemblance(Article a)
        {
            return CalculateResemblance(this, a);
        }

        
        /// <summary>
        /// Indique si les conditions sont réunies pour permettre
        /// d'appeler la méthode Matches.
        /// </summary>
        /// <param name="a">L'article avec lequel on veut pouvoir utiliser la fonction Matches</param>
        /// <returns>true si le résultat de Matches aura du sens avec l'Article passé en paramètre. Sinon, retourne false,
        /// et alors Matches ne retournera pas de résultat fiable/exploitable.</returns>
        /// <see cref="Matches(Article)"/>
        internal bool CanMatch(Article a)
        {
            if ((string.IsNullOrEmpty(Identifier) == false) && (string.IsNullOrEmpty(a.Identifier) == false))
            {
                return true;
            }

            if ((string.IsNullOrEmpty(BrowserUrl) == false) && (string.IsNullOrEmpty(a.BrowserUrl) == false))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Indique si l'Article passé en paramètre possède le même contenu
        /// que l'article en cours. Sont évalués (dans cet ordre):
        /// Identifier puis BrowserUrl.
        /// </summary>
        /// <param name="a">Article à comparer avec l'objet courant.</param>
        /// <returns>
        /// Retourne true, s'il y a bien une correspondance exacte entre
        /// l'Article en paramètre et l'objet courant. 
        /// Attention, par contre, si Matches renvoie false. Cela peut indiquer
        /// la non correspondance entre les deux objets, mais cela peut aussi indiquer
        /// que l'un ou l'autre des deux objets n'a pas les propriétés nécessaires
        /// pour effectuer la comparaison. Il convient de faire la distinction en utilisant
        /// la méthode CanMatch.
        /// </returns>
        /// <see cref="CanMatch(Article)"/>
        internal bool Matches(Article a)
        {
            if ((string.IsNullOrEmpty(Identifier) == false) && (string.IsNullOrEmpty(a.Identifier) == false))
            {
                if (string.CompareOrdinal(Identifier, a.Identifier) == 0)
                {
                    return true;
                }
            }

            if ((string.IsNullOrEmpty(BrowserUrl) == false) && (string.IsNullOrEmpty(a.BrowserUrl) == false))
            {
                if (string.Compare(BrowserUrl, a.BrowserUrl, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        /// <summary>
        /// Enumération des clés que cette classe utilise
        /// pour insérer/supprimer des notifications dans
        /// NotificationSystem.
        /// </summary>
        enum NotificationKeys
        {
            /// <summary>
            /// Une erreur est survenue durant la mise à jour de
            /// l'article. Mauvaise page web ? Erreur de template ?
            /// </summary>
            /// <see cref="Update(XPathNavigator, ArticleTemplate)"/>
            ArticleUpdateError
        }
    }

}
