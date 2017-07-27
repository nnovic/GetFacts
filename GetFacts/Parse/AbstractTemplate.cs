using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <seealso cref="ShouldSerializeIdentifierTemplate"/>
        [JsonProperty(Order =500)]
        public readonly StringTemplate IdentifierTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir le titre de la page/section/article
        /// </summary>
        /// <remarks>Une page/section/article qui n'aurait ni titre ni texte pourrait
        /// être jugée non-affichable à l'écran!</remarks>
        /// <seealso cref="AbstractInfo.HasContent"/>
        /// <see cref="AbstractInfo.Title"/>
        /// <seealso cref="ShouldSerializeTitleTemplate"/>
        [JsonProperty(Order =500)]
        public readonly StringTemplate TitleTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir le texte de la page/section/article
        /// </summary>
        /// <remarks>Une page/section/article qui n'aurait ni titre ni texte pourrait
        /// être jugée non-affichable à l'écran!</remarks>
        /// <seealso cref="AbstractInfo.HasContent"/>
        /// <see cref="AbstractInfo.Text"/>
        /// <seealso cref="ShouldSerializeTextTemplate"/>
        [JsonProperty(Order = 500)]
        public readonly StringTemplate TextTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir l'URL d'une image qui accompagnera le tite et/ou le texte.
        /// L'image sera téléchargée avec une prioriété inférieure aux pages, mais
        /// supérieures aux autres médias.
        /// </summary>
        /// <seealso cref="ShouldSerializeIconUrlTemplate"/>
        [JsonProperty(Order = 500)]
        public readonly StringTemplate IconUrlTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir l'URL d'un média (vidéo ou musique) qui sera joué
        /// à la demande de l'utilisateur, en rapport avec la page/section/article en
        /// cours.
        /// </summary>
        /// <seealso cref="ShouldSerializeMediaUrlTemplate"/>
        [JsonProperty(Order = 500)]
        public readonly StringTemplate MediaUrlTemplate = new StringTemplate();

        /// <summary>
        /// Template pour obtenir l'URL qui s'ouvrira dans le web browser
        /// si l'utilisateur clique sur la page/section/article.
        /// </summary>
        /// <seealso cref="ShouldSerializeBrowserUrlTemplate"/>
        [JsonProperty(Order = 500)]
        public readonly StringTemplate BrowserUrlTemplate = new StringTemplate();


        #region json conditional serialization

        public bool ShouldSerializeIdentifierTemplate()
        {
            return !IdentifierTemplate.IsNullOrEmpty;
        }

        public bool ShouldSerializeTitleTemplate()
        {
            return !TitleTemplate.IsNullOrEmpty;
        }

        public bool ShouldSerializeTextTemplate()
        {
            return !TextTemplate.IsNullOrEmpty;
        }

        public bool ShouldSerializeIconUrlTemplate()
        {
            return !IconUrlTemplate.IsNullOrEmpty;
        }

        public bool ShouldSerializeMediaUrlTemplate()
        {
            return !MediaUrlTemplate.IsNullOrEmpty;
        }

        public bool ShouldSerializeBrowserUrlTemplate()
        {
            return !BrowserUrlTemplate.IsNullOrEmpty;
        }

        #endregion        
    }
}
