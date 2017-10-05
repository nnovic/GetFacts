using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Render
{
    /// <summary>
    /// Cette interface est définie pour les éléments graphiques
    /// qui affichent des informations collectées sur Internet.
    /// </summary>
    public interface IHostsInformation
    {
        /// <summary>
        /// Retourne true si une ou plusieurs informations
        /// de l'objet qui implémente l'interface sont "nouvelles",
        /// dans le sens où elles ont été récemment collectées
        /// sur Internet et l'utilisateur n'a pas réalisé d'action
        /// laissant penser qu'il a pris connaissance de ces informations.
        /// La sémentique de cette prorpiété est donc légèrement 
        /// différente de celle de AbstractInfo.IsNew
        /// </summary>
        bool HasNewInformation { get; }

        /// <summary>
        /// Retourne un texte qui résume le contenu de l'information
        /// hébergée dans l'objet qui implémente cette interface.
        /// </summary>
        string InformationHeadline { get; }

        /// <summary>
        /// Retourne un résumé de l'information
        /// hébergée dans l'objet qui implémente cette interface.
        /// </summary>
        string InformationSummary { get; }
    }
}
