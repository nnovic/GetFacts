using GetFacts.Parse;
using System;
using System.Collections.Generic;
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

namespace TemplatesApp
{
    /// <summary>
    /// Logique d'interaction pour PageTemplateEditor.xaml
    /// </summary>
    public partial class PageTemplateEditor : UserControl
    {
        public PageTemplateEditor()
        {
            InitializeComponent();
        }

        #region PageTemplate

        private PageTemplate pageTemplate = null;

        public PageTemplate PageTemplate
        {
            get
            {
                return pageTemplate;
            }
            set
            {
                ClearTemplate();
                pageTemplate = value;
                InitTemplate();
            }
        }

        private void ClearTemplate()
        {

        }

        private void InitTemplate()
        {
            PageNameInput.Text = PageTemplate.PageName;
            PageTypeSelector.SelectedItem = PageTemplate.PageType;
            ReferenceInput.Text = PageTemplate.Reference;
            CharsetSelector.SelectedItem = PageTemplate.Encoding;
            TitleTemplateEditor.StringTemplate = PageTemplate.TitleTemplate;
            TextTemplateEditor.StringTemplate = PageTemplate.TextTemplate;
            IconTemplateEditor.StringTemplate = PageTemplate.IconUrlTemplate;
            MediaTemplateEditor.StringTemplate = PageTemplate.MediaUrlTemplate;
        }

        #endregion


        private void AddSectionButton_Click(object sender, RoutedEventArgs e)
        {
            SectionTemplate newSection = new SectionTemplate();
            PageTemplate.Sections.Add(newSection);
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            PageTypeSelector.Items.Clear();
            foreach(string type in AbstractParser.AvailableParsers() )
            {
                PageTypeSelector.Items.Add(type);
            }
            PageTypeSelector.SelectedItem = AbstractParser.DefaultParser;

            CharsetSelector.Items.Clear();
            foreach(EncodingInfo ei in Encoding.GetEncodings())
            {                
                CharsetSelector.Items.Add(ei.GetEncoding());
            }
        }
    }
}
