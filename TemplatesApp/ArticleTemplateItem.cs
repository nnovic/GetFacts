using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TemplatesApp
{
    class ArticleTemplateItem:TreeViewItem
    {
        private readonly ArticleTemplate articleTemplate;
        private readonly ArticleTemplateEditor templateEditor;

        public ArticleTemplateItem(ArticleTemplate template)
        {
            this.articleTemplate = template;

            templateEditor = new ArticleTemplateEditor() { ArticleTemplate = articleTemplate };
            Header = templateEditor;

        }

        internal ArticleTemplate ArticleTemplate
        {
            get { return articleTemplate; }
        }
    }
}
