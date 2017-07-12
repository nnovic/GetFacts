using GetFacts.Parse;
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
            ConfigTree.Items.Add(pte);
        }

        #endregion

    }
}
