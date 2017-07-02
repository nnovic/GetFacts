using BusinessLogic.Behaviour.Other.SecuritiesSearch;
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
        internal void Update(Article source)
        {
            UpdateInfo(source);
        }

        internal void Update(XPathNavigator nav, ArticleTemplate template)
        {
            UpdateInfo(nav, template);
        }
        
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
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        internal bool HasMatchingIdentifiers(Article a)
        {
            if ((string.IsNullOrEmpty(Identifier) == false) && (string.IsNullOrEmpty(a.Identifier) == false))
            {
                if( string.CompareOrdinal(Identifier,a.Identifier)==0 )
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
    }

}
