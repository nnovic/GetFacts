using GetFacts.Parse;
using GetFacts.Render;
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
        private bool templateDataReady = false;
        private bool pageDataReady = false;

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
            if( Workflow.IsTemplateDataAvailable != templateDataReady )
            {
                templateDataReady = workflow.IsTemplateDataAvailable;
                if (templateDataReady)
                {
                    InitTemplateTree();
                }
                else
                {
                    ClearTemplateTree();
                    ClearPreviewTree();
                }
            }

            if (Workflow.IsPageDataAvailable != pageDataReady)
            {
                pageDataReady = workflow.IsPageDataAvailable;
                if (pageDataReady)
                {
                    InitPreviewTree();
                }
                else
                { 
                    ClearPreviewTree();
                }
            }
        }




        #region template


        private void ClearTemplateTree()
        {
            ConfigTree.Items.Clear();
        }

        private void InitTemplateTree()
        {
            PageTemplateEditor pte = new PageTemplateEditor() { PageTemplate = Workflow.PageTemplate };
            TreeViewItem pageRoot = new TreeViewItem() { Header = pte };
            ConfigTree.Items.Add(pageRoot);

            foreach (SectionTemplate st in Workflow.PageTemplate.Sections)
            {
                SectionTemplateEditor ste = new SectionTemplateEditor() { SectionTemplate = st };
                TreeViewItem sectionNode = new TreeViewItem() { Header = ste };
                pageRoot.Items.Add(sectionNode);

                foreach (ArticleTemplate at in st.Articles)
                {
                    ArticleTemplateEditor ate = new ArticleTemplateEditor() { ArticleTemplate = at };
                    TreeViewItem articleLeaf = new TreeViewItem() { Header = ate };
                    sectionNode.Items.Add(articleLeaf);
                }
            }

            pageRoot.ExpandSubtree();
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



        #region test template

        private void TestTemplateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ClearPreviewTree();

            // ANALYSE

            GetFacts.Facts.Page page = new GetFacts.Facts.Page(Workflow.PageTemplate.Reference);
            page.Template = Workflow.PageTemplate;
            page.Parser = AbstractParser.NewInstance(Workflow.PageTemplate.PageType);
            page.Update(Workflow.DownloadTask.LocalFile);

            // PAGE

            ArticleDisplay pageDisplay = new ArticleDisplay(false, 0) {
                DesiredOrientation = ArticleDisplay.Orientation.Horizontal,
                Width = 500, Height=200
            };
            pageDisplay.Update(page);
            TreeViewItem rootItem = new TreeViewItem() { Header = pageDisplay };

            // SECTIONS

            int sectionsCount = page.SectionsCount;
            for(int sectionIndex=0; sectionIndex<sectionsCount; sectionIndex++)
            {
                GetFacts.Facts.Section section = page.GetSection(sectionIndex);
                ArticleDisplay sectionDisplay = new ArticleDisplay(false, 0) {
                    DesiredOrientation = ArticleDisplay.Orientation.Horizontal,
                    Width=500, Height=200
                };
                sectionDisplay.Update(section);
                TreeViewItem sectionItem = new TreeViewItem() { Header = sectionDisplay };
                rootItem.Items.Add(sectionItem);

                // ARTICLES

                int articlesCount = section.ArticlesCount;
                for(int articleIndex=0; articleIndex<articlesCount; articleIndex++)
                {
                    GetFacts.Facts.Article article = section.GetArticle(articleIndex);
                    ArticleDisplay articleDisplay = new ArticleDisplay(false, 0) {
                        DesiredOrientation = ArticleDisplay.Orientation.Horizontal,
                        Width=500, Height=200
                    };
                    articleDisplay.Update(article);
                    TreeViewItem articleItem = new TreeViewItem() { Header =articleDisplay };
                    sectionItem.Items.Add(articleItem);
                }
            }

            PreviewTree.Items.Add(rootItem);
            rootItem.ExpandSubtree();
            //page.Parser = Parser;
            //page.Template = PageTemplate;



        }


        private void InitPreviewTree()
        {

        }

        private void ClearPreviewTree()
        {
            PreviewTree.Items.Clear();
        }

        #endregion


    }
}
