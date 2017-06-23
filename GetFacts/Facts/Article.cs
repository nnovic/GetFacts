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



        /*
        internal int CompareTo(Article a )
        {
            return Compare(this, a);
        }

        static int Compare(Article a1, Article a2)
        {
            const int HIGH_SCORE = 20;
            const int TITLE_SCORE = 10;
            const int TEXT_SCORE = 10;

            // compare internal identifiers;
            // equality results in highest score
            if( string.Compare(a1.Identifier, a2.Identifier) == 0 )
            {
                if ((string.IsNullOrEmpty(a1.Identifier) == false) && (string.IsNullOrEmpty(a2.Identifier) == false))
                    return HIGH_SCORE;
            }

            // now, start from high score and
            // remove points for every word differences
            int score = HIGH_SCORE;

            score -= (int)(TITLE_SCORE*ErrorsCount(a1.Title, a2.Title));
            score -= (int)(TEXT_SCORE*ErrorsCount(a1.Text, a2.Title));

            score = Math.Min(score, HIGH_SCORE); // do not return a score greater than 20
            score = Math.Max(score, 0); // do not return a score below 0
            return score;
        }

        static double ErrorsCount(string s1, string s2)
        {
            int count = 0;
            char[] whiteSpaces = new char[] { ' ', '\t', '\r', '\n' };
            string[] words1 = s1.Split(whiteSpaces);
            List<string> words2 = s2.Split(whiteSpaces).ToList();

            int index2 = 0;
            int minIndex = Math.Min(words1.Length, words2.Count);
            int maxIndex = Math.Max(words1.Length, words2.Count);

            for (int index1 = 0; index1 <minIndex; index1++)
            {
                string word1 = words1[index1];
                string word2 = words2[index2];

                // comparison: 
                if ( string.Compare(word1, word2)==0)
                {
                    index2++;
                }

                // slow comparison:
                else
                {
                    int newIndex = words2.IndexOf(word1, index2);
                    if(newIndex==-1)
                    {
                        // the word in s1 does not exist in s2.
                        // counts as 1 error.
                        count++;
                    }
                    else
                    {
                        count += Math.Abs(newIndex - index2);
                        index2 = newIndex;
                        index2++;
                    }

                }
            }

            count +=maxIndex-minIndex;
            double result = (double)count / (double)maxIndex;
            return result;
        }
        */

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
