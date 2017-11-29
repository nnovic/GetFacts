using GetFacts;
using GetFacts.Parse;
using GetFacts.Render;
using System;
using System.Windows.Controls;
using System.Windows.Media;

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
            PageTemplateItem pageRoot = new PageTemplateItem(Workflow.PageTemplate);
            ConfigTree.Items.Add(pageRoot);

            foreach (SectionTemplate st in Workflow.PageTemplate.Sections)
            {
                SectionTemplateItem sectionFolder = new SectionTemplateItem(st);
                pageRoot.Items.Add(sectionFolder);

                foreach (ArticleTemplate at in st.Articles)
                {
                    /*ArticleTemplateEditor ate = new ArticleTemplateEditor() { ArticleTemplate = at };
                    TreeViewItem articleLeaf = new TreeViewItem() { Header = ate };*/
                    ArticleTemplateItem articleLeaf = new ArticleTemplateItem(at);
                    sectionFolder.Items.Add(articleLeaf);
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

        const double PREVIEW_WIDTH = 300;
        const double PREVIEW_HEIGHT = 150;

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
                Width = PREVIEW_WIDTH, Height= PREVIEW_HEIGHT
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
                    Width= PREVIEW_WIDTH, Height= PREVIEW_HEIGHT
                };
                sectionDisplay.Update(section);
                TreeViewItem sectionItem = new TreeViewItem() { Header = sectionDisplay };
                rootItem.Items.Add(sectionItem);
                sectionItem.IsExpanded = true;

                // ARTICLES

                int articlesCount = section.ArticlesCount;
                for(int articleIndex=0; articleIndex<articlesCount; articleIndex++)
                {
                    GetFacts.Facts.Article article = section.GetArticle(articleIndex);
                    ArticleDisplay articleDisplay = new ArticleDisplay(false, 0) {
                        DesiredOrientation = ArticleDisplay.Orientation.Horizontal,
                        Width= PREVIEW_WIDTH,
                        Height = PREVIEW_HEIGHT
                    };
                    articleDisplay.Update(article);
                    if (article.HasContent == false) articleDisplay.Background = Brushes.Gray;
                    TreeViewItem articleItem = new TreeViewItem() { Header =articleDisplay };
                    sectionItem.Items.Add(articleItem);
                }


                if( (articlesCount>0) || (section.HasContent==true))
                {
                    rootItem.IsExpanded = true;
                }
            }

            PreviewTree.Items.Add(rootItem);
            //rootItem.ExpandSubtree();
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

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TemplateFactory.GetInstance().SaveTemplate(Workflow.PageTemplate, Workflow.TemplateFile);
        }
    }
}
