using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    /// <summary>
    /// Méthodes d'extension qui concernent des classes du namespace GetFacts.Parse
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Recherche, dans une liste d'objets XPathAttribute, 
        /// le premier dont la propriété Name est identique à
        /// "attributeName".
        /// </summary>
        /// <param name="attributes">liste d'objets XPathAttribute dans laquelle faire la recherche</param>
        /// <param name="attributeName">nom de l'attribut à trouver. Doit correspondre au champ XpathAttribute.Name de l'un des objets de la liste.</param>
        /// <returns>Le premir XPathAttribute avec le bon nom. Retourne null si aucune correspondance n'a été trouvée.</returns>
        /// <remarks>ceci est une méthode d'extension</remarks>
        public static XPathAttribute Find(this IList<XPathAttribute> attributes, string attributeName)
        {
            foreach (XPathAttribute attribute in attributes)
            {
                if (string.Compare(attribute.Name, attributeName) == 0)
                    return attribute;
            }

            return null;
        }


        /// <summary>
        /// Retourne le dernier HtmlNode de la collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static HtmlNode Last(this HtmlNodeCollection collection)
        {
            if( (collection==null) || (collection.Count==0))
            {
                return null;
            }
            return collection[collection.Count - 1];
        }
    }

}
