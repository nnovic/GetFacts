using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GetFacts
{
    /// <summary>
    /// Suivez les étapes 1a ou 1b puis 2 pour utiliser ce contrôle personnalisé dans un fichier XAML.
    ///
    /// Étape 1a) Utilisation de ce contrôle personnalisé dans un fichier XAML qui existe dans le projet actif.
    /// Ajoutez cet attribut XmlNamespace à l'élément racine du fichier de balisage où il doit 
    /// être utilisé :
    ///
    ///     xmlns:MyNamespace="clr-namespace:GetFacts"
    ///
    ///
    /// Étape 1b) Utilisation de ce contrôle personnalisé dans un fichier XAML qui existe dans un autre projet.
    /// Ajoutez cet attribut XmlNamespace à l'élément racine du fichier de balisage où il doit 
    /// être utilisé :
    ///
    ///     xmlns:MyNamespace="clr-namespace:GetFacts;assembly=GetFacts"
    ///
    /// Vous devrez également ajouter une référence du projet contenant le fichier XAML
    /// à ce projet et régénérer pour éviter des erreurs de compilation :
    ///
    ///     Cliquez avec le bouton droit sur le projet cible dans l'Explorateur de solutions, puis sur
    ///     "Ajouter une référence"->"Projets"->[Recherchez et sélectionnez ce projet]
    ///
    ///
    /// Étape 2)
    /// Utilisez à présent votre contrôle dans le fichier XAML.
    ///
    ///     <MyNamespace:TextBoxWithValidation/>
    ///
    /// </summary>
    public abstract class TextBoxWithValidation : TextBox
    {
        private bool isValid = true;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>The value of IsValid must not be trusted if the
        /// Text of the TextBox is null or Empty.</remarks>
        public bool IsValid
        {
            get { return isValid; }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {            
            ValidateInput();
            base.OnTextChanged(e);
        }

        private void ValidateInput()
        {
            string newInput = Text;
            if( string.IsNullOrEmpty(newInput) )
            {
                ApplyDefaultStyle();
                return;
            }

            try
            {
                if (IsTextValid(newInput))
                {
                    isValid = true;
                    ApplyValidInputStyle();                    
                }
                else
                {
                    isValid = false;
                    ApplyInvalidInputStyle();
                }
            }
            catch
            {
                isValid = false;
                ApplyInputErrorStyle();
            }
        }

        private void ApplyDefaultStyle()
        {
            Foreground = SystemColors.ControlTextBrush;
        }

        private void ApplyValidInputStyle()
        {
            Foreground = Brushes.Green;
        }

        private void ApplyInvalidInputStyle()
        {
            Foreground = Brushes.Red;
        }

        private void ApplyInputErrorStyle()
        {
            Foreground = Brushes.Red;
        }

        protected abstract bool IsTextValid(string text);
    }
}
