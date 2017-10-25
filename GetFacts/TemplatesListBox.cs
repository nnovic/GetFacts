using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    /// à ce projet et regénérer pour éviter des erreurs de compilation :
    ///
    ///     Cliquez avec le bouton droit sur le projet cible dans l'Explorateur de solutions, puis sur
    ///     "Ajouter une référence"->"Projets"->[Recherchez et sélectionnez ce projet]
    ///
    ///
    /// Étape 2)
    /// Utilisez à présent votre contrôle dans le fichier XAML.
    ///
    ///     <MyNamespace:TemplatesListBox/>
    ///
    /// </summary>
    public class TemplatesListBox : ListBox
    {
        static TemplatesListBox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(TemplatesListBox), new FrameworkPropertyMetadata(typeof(TemplatesListBox)));
        }


        #region TemplatesDirectoryProperty

        static FrameworkPropertyMetadata propertymetadata = new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, 
            new PropertyChangedCallback(TemplatesDirectory_PropertyChanged),
            new CoerceValueCallback(TemplatesDirectory_CoerceValue),
            false, 
            UpdateSourceTrigger.PropertyChanged);

        public static readonly DependencyProperty TemplatesDirectoryProperty = DependencyProperty.Register(
            "TemplatesDirectory", 
            typeof(string), 
            typeof(TemplatesListBox),
            propertymetadata,
            new ValidateValueCallback(TemplatesDirectory_Validate));
        
        public string TemplatesDirectory
        {
            get
            {
                return this.GetValue(TemplatesDirectoryProperty) as string;
            }
            set
            {
                this.SetValue(TemplatesDirectoryProperty, value);
            }
        }

        private static void TemplatesDirectory_PropertyChanged(
            DependencyObject dobj,
            DependencyPropertyChangedEventArgs e)
        {
            TemplatesListBox lbox = (TemplatesListBox)dobj;
            string oldPath = (string)e.OldValue;
            string newPath = (string)e.NewValue;

            lbox.Items.Clear();
            if (string.IsNullOrEmpty(newPath) == false)
            {
                List<string> templates = TemplateFactory.CreateTemplatesList(newPath);
                templates.ForEach(t => lbox.Items.Add(t));
            }

            //To be called whenever the DP is changed.
            //MessageBox.Show(string.Format(
            //   "Property changed is fired : OldValue {0} NewValue : {1}", e.OldValue, e.NewValue));
        }


        /*private void RefreshFilesList()
        {            
            try
            {
                FilesList.Items.Clear();
                string dir = TemplatesDirSelection.SelectedItem as string;
                List<string> templates = TemplateFactory.CreateTemplatesList(dir);
                templates.ForEach(t => FilesList.Items.Add(t));
            }
            catch (DirectoryNotFoundException)
            {
            }
        }*/

        private static object TemplatesDirectory_CoerceValue(
            DependencyObject dobj, 
            object Value)
        {
            //called whenever dependency property value is reevaluated. The return value is the
            //latest value set to the dependency property
            //MessageBox.Show(string.Format("CoerceValue is fired : Value {0}", Value));
            return Value;
        }

        private static bool TemplatesDirectory_Validate(object Value)
        {
            string path = Value as string;
            if (string.IsNullOrEmpty(path))
                return true;
            return Directory.Exists(path);
            //Custom validation block which takes in the value of DP
            //Returns true / false based on success / failure of the validation
            //MessageBox.Show(string.Format("DataValidation is Fired : Value {0}", Value));
            //return true;
        }


        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}
