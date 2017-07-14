using GetFacts.Parse;
using System;
using System.Windows.Controls;

namespace TemplatesApp
{
    /// <summary>
    /// Logique d'interaction pour TemplateEditor.xaml
    /// </summary>
    public partial class TemplateEditor : UserControl
    {
        public TemplateEditor()
        {
            InitializeComponent();
        }

        #region get/set template

        private PageTemplate pageTemplate = null;

        public PageTemplate PageTemplate
        {
            get
            {
                return pageTemplate;
            }
            set
            {
                ClearTreeView();
                pageTemplate = value;
                PopulateTreeView();

            }
        }

        private void ClearTreeView()
        {

        }

        private void PopulateTreeView()
        {
            PageTemplateEditor pte = new PageTemplateEditor() { PageTemplate=PageTemplate };
            TreeViewItem pageRoot = new TreeViewItem() { Header = pte };
            ConfigTree.Items.Add(pageRoot);

            foreach(SectionTemplate st in PageTemplate.Sections)
            {
                SectionTemplateEditor ste = new SectionTemplateEditor() { SectionTemplate = st };
                TreeViewItem sectionNode = new TreeViewItem() { Header = ste };
                pageRoot.Items.Add(sectionNode);

                foreach(ArticleTemplate at in st.Articles)
                {
                    ArticleTemplateEditor ate = new ArticleTemplateEditor() { ArticleTemplate = at };
                    TreeViewItem articleLeaf = new TreeViewItem() { Header = ate };
                    sectionNode.Items.Add(articleLeaf);
                }

            }
        }

        #endregion
    }
}
