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

        internal void Update(XPathNavigator nav, SectionTemplate sectionTemplate)
        {
            UpdateInfo(nav, sectionTemplate);

            foreach (ArticleTemplate articleTemplate in sectionTemplate.Articles)
            {
                XPathNodeIterator iter = nav.Select(articleTemplate.XPathFilter);
                while(iter.MoveNext())
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
            Article bestCandidate = null;
            const double minimumScore = 0.7;

            foreach (Article a in Children)
            {
                if( tmp.HasMatchingIdentifiers(a) )
                {
                    a.Update(tmp);
                    return;
                }

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
    }
}
