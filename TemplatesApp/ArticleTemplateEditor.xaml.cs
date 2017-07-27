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
    /// Logique d'interaction pour ArticleTemplateEditor.xaml
    /// </summary>
    public partial class ArticleTemplateEditor : UserControl
    {
        public ArticleTemplateEditor()
        {
            InitializeComponent();
        }

        #region ArticleTemplate

        private ArticleTemplate articleTemplate = null;

        public ArticleTemplate ArticleTemplate
        {
            get
            {
                return articleTemplate;
            }
            set
            {
                ClearTemplate();
                articleTemplate = value;
                InitTemplate();
            }
        }

        private void ClearTemplate()
        {

        }

        private void InitTemplate()
        {
            XPathFilterInput.Text = ArticleTemplate.XPathFilter;
            TitleTemplateEditor.StringTemplate = ArticleTemplate.TitleTemplate;
            TextTemplateEditor.StringTemplate = ArticleTemplate.TextTemplate;
            IconTemplateEditor.StringTemplate = ArticleTemplate.IconUrlTemplate;
            MediaTemplateEditor.StringTemplate = ArticleTemplate.MediaUrlTemplate;
            BrowserUrlTemplateEditor.StringTemplate = ArticleTemplate.BrowserUrlTemplate;
        }

        #endregion

        private void DeleteThisArticleButton_Click(object sender, RoutedEventArgs e)
        {
            ArticleTemplateItem ati = (ArticleTemplateItem)this.Parent;
            SectionTemplateItem sti = (SectionTemplateItem)ati.Parent;
            sti.SectionTemplate.Articles.Remove(ArticleTemplate);
        }

        private void XPathFilterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (XPathFilterInput.IsValid)
                ArticleTemplate.XPathFilter = XPathFilterInput.Text;
        }
    }
}
