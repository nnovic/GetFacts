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
        private Workflow workflow = null;
        private bool isReady = false;

        public TemplateEditor()
        {
            InitializeComponent();
        }

        internal Workflow Workflow
        {
            get
            {
                return workflow;
            }
            set
            {
                workflow = value;
                workflow.WorkflowUpdated += Workflow_WorkflowUpdated;
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void Workflow_WorkflowUpdated(object sender, EventArgs e)
        {
            if (workflow.IsReadyForTemplateEditor != isReady)
            {
                isReady = workflow.IsReadyForTemplateEditor;
                if (isReady)
                {
                    InitTemplateEditor();
                }
                else
                {
                    ClearTemplateEditor();
                }
            }
        }




        #region get/set template

        private void InitTemplateEditor()
        {
            // TODO
        }

        private void ClearTemplateEditor()
        {
            // TODO
        }

        /*
        private PageTemplate pageTemplate = null;

        public PageTemplate PageTemplate
        {
            get
            {
                return pageTemplate;
            }
            set
            {
                ClearTemplateTree();
                pageTemplate = value;
                PopulateTemplateTree();

            }
        }

        private void ClearTemplateTree()
        {

        }

        private void PopulateTemplateTree()
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
        */
        #endregion


        #region other parameters
        /*
                internal string Url
                {
                    get;set;
                }

                internal AbstractParser Parser
                {
                    get;set;
                }
                */
        #endregion


        #region test template

        private void TestTemplateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //ClearPreviewTree();
            //GetFacts.Facts.Page page = new GetFacts.Facts.Page(Url);
            //page.Parser = Parser;
            //page.Template = PageTemplate;


        }

        /* private void ClearPreviewTree()
         {
             PreviewTree.Items.Clear();
         }*/

        #endregion


    }
}
