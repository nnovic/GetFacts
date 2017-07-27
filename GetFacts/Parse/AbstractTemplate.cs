using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Parse
{
    /// <summary>
    /// Fournit les propriétés communes à tous les templates: ceux
    /// pour la page, les sections et les articles.
    /// </summary>
    public class AbstractTemplate
    {
        /// <summary>
        /// Template qui permet d'extraire une chaine de caractère qui est
        /// identifie de façon unique et sans équivoque la page/section/article.
        /// C'est le moyen idéal pour GetFacts de différencier les nouveaux
        /// articles à créer des articles existants à rafraîchir.
        /// </summary>
        /// <see cref="AbstractInfo.Identifier"/>
        public readonly StringTemplate IdentifierTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir le titre de la page/section/article
        /// </summary>
        /// <remarks>Une page/section/article qui n'aurait ni titre ni texte pourrait
        /// être jugée non-affichable à l'écran!</remarks>
        /// <seealso cref="AbstractInfo.HasContent"/>
        /// <see cref="AbstractInfo.Title"/>
        public readonly StringTemplate TitleTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir le texte de la page/section/article
        /// </summary>
        /// <remarks>Une page/section/article qui n'aurait ni titre ni texte pourrait
        /// être jugée non-affichable à l'écran!</remarks>
        /// <seealso cref="AbstractInfo.HasContent"/>
        /// <see cref="AbstractInfo.Text"/>
        public readonly StringTemplate TextTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir l'URL d'une image qui accompagnera le tite et/ou le texte.
        /// L'image sera téléchargée avec une prioriété inférieure aux pages, mais
        /// supérieures aux autres médias.
        /// </summary>
        public readonly StringTemplate IconUrlTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir l'URL d'un média (vidéo ou musique) qui sera joué
        /// à la demande de l'utilisateur, en rapport avec la page/section/article en
        /// cours.
        /// </summary>
        public readonly StringTemplate MediaUrlTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir l'URL qui s'ouvrira dans le web browser
        /// si l'utilisateur clique sur la page/section/article.
        /// </summary>
        public readonly StringTemplate BrowserUrlTemplate = new StringTemplate();

        protected virtual bool CompareTo(AbstractTemplate at)
        {
            if (StringTemplate.CompareTo(IdentifierTemplate, at.IdentifierTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(TitleTemplate,at.TitleTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(TextTemplate,at.TextTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(IconUrlTemplate,at.IconUrlTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(MediaUrlTemplate,at.MediaUrlTemplate) == false)
                return false;

            if (StringTemplate.CompareTo(BrowserUrlTemplate,at.BrowserUrlTemplate) == false)
                return false;

            return true;
        }
    }
}
