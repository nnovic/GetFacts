using BusinessLogic.Behaviour.Other.SecuritiesSearch;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace GetFacts.Facts
{
    public class Section : AbstractInfo
    {
        internal Section(string name):base(name)
        {

        }

        /// <summary>
        /// Destruction de cet objet:
        /// - s'assurer que toutes les notifications poussées dans
        ///   NotificationSystem par cet objet soient supprimées.
        /// </summary>
        ~Section()
        {
            NotificationSystem.GetInstance().RemoveAll(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="sectionTemplate"></param>
        /// <remarks>Ajoute une notification dans NotificationSystem si erreur durant
        /// la mise à jour de la section. Ne bloque pas les exceptions.</remarks>
        /// <seealso cref="NotificationKeys.SectionUpdateError"/>
        internal void Update(XPathNavigator nav, SectionTemplate sectionTemplate)
        {
            var notification = new NotificationSystem.Notification(this,
                (int)NotificationKeys.SectionUpdateError)
            {
                Title = Identifier,
                Description = "Update error."
            };

            try
            {
                UpdateInfo(nav, sectionTemplate);

                foreach (ArticleTemplate articleTemplate in sectionTemplate.Articles)
                {
                    XPathNodeIterator iter;
                    if (string.IsNullOrEmpty(articleTemplate.XPathFilter)) { iter = nav.SelectChildren(XPathNodeType.Element); }
                    else { iter = nav.Select(articleTemplate.XPathFilter); }

                    while (iter.MoveNext())
                    {
                        XPathNavigator subTree = iter.Current;
                        Article tmp = new Article()
                        {
                            BaseUri = BaseUri
                        };
                        tmp.Update(subTree, articleTemplate);
                        AddOrUpdateArticle(tmp);
                    }

                }

                NotificationSystem.GetInstance().Remove(notification);
            }
            catch
            {
                NotificationSystem.GetInstance().Add(notification);
                throw;
            }
        }

        internal void RemoveArticle(Article a)
        {
            if (a == null) throw new ArgumentNullException();
            if (Children.Contains(a) == false) throw new ArgumentException();
            Children.Remove(a);
        }

        private void AddOrUpdateArticle(Article tmp)
        {
            double highestScore = 0;            
            const double minimumScore = 0.7;
            List<Article> articlesThatCannotMatch = new List<Article>();

            foreach (Article a in Children)
            {
                if (tmp.CanMatch(a))
                {
                    if (tmp.Matches(a))
                    {
                        a.Update(tmp);
                        return;
                    }
                }
                else
                {
                    articlesThatCannotMatch.Add(a);
                }
            }

            if (articlesThatCannotMatch.Any() == false)
            {
                Children.Add(tmp);
            }

            else
            {
                Article bestCandidate = null;

                foreach (Article a in articlesThatCannotMatch)
                {
                    double articleScore = tmp.CalculateResemblance(a);
                    if (articleScore >= minimumScore)
                    {
                        if (articleScore > highestScore)
                        {
                            highestScore = articleScore;
                            bestCandidate = a;
                        }
                    }
                }

                if (bestCandidate == null)
                {
                    Children.Add(tmp);
                }
                else
                {
                    bestCandidate.Update(tmp);
                }
            }
        }


        #region Articles

        public int ArticlesCount
        {
            get { return Children.Count; }
        }

        public Article GetArticle(int index)
        {
            return (Article)Children[index];
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
            /// la section. Mauvaise page web ? Erreur de template ?
            /// </summary>
            /// <see cref="Update(XPathNavigator, SectionTemplate)"/>
            SectionUpdateError
        }
    }
}
