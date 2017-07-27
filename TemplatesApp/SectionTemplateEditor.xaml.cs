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
    /// Logique d'interaction pour SectionTemplateEditor.xaml
    /// </summary>
    public partial class SectionTemplateEditor : UserControl
    {
        public SectionTemplateEditor()
        {
            InitializeComponent();
        }

        #region SectionTemplate

        private SectionTemplate sectionTemplate = null;

        public SectionTemplate SectionTemplate
        {
            get
            {
                return sectionTemplate;
            }
            set
            {
                ClearTemplate();
                sectionTemplate = value;
                InitTemplate();
            }
        }

        private void ClearTemplate()
        {

        }

        private void InitTemplate()
        {
            SectionNameInput.Text = SectionTemplate.SectionName;
            XPathFilterInput.Text = SectionTemplate.XPathFilter;
            TitleTemplateEditor.StringTemplate = SectionTemplate.TitleTemplate;
            TextTemplateEditor.StringTemplate = SectionTemplate.TextTemplate;
            IconTemplateEditor.StringTemplate = SectionTemplate.IconUrlTemplate;
            MediaTemplateEditor.StringTemplate = SectionTemplate.MediaUrlTemplate;
        }

        #endregion

        private void AddArticleButton_Click(object sender, RoutedEventArgs e)
        {
            ArticleTemplate newArticle = new ArticleTemplate();
            SectionTemplate.Articles.Add(newArticle);
        }

        private void DeleteThisSectionButton_Click(object sender, RoutedEventArgs e)
        {
            SectionTemplateItem sti = (SectionTemplateItem)this.Parent;
            PageTemplateItem pti = (PageTemplateItem)sti.Parent;
            pti.PageTemplate.Sections.Remove(SectionTemplate);
        }

        private void SectionNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            SectionTemplate.SectionName = SectionNameInput.Text;
        }

        private void XPathFilterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (XPathFilterInput.IsValid)
                SectionTemplate.XPathFilter = XPathFilterInput.Text;
        }
    }
}
