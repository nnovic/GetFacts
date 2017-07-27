using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TemplatesApp
{
    class SectionTemplateItem:TreeViewItem
    {
        private readonly SectionTemplate sectionTemplate;
        private readonly SectionTemplateEditor templateEditor;

        public SectionTemplateItem(SectionTemplate template)
        {
            this.sectionTemplate = template;

            templateEditor = new SectionTemplateEditor() { SectionTemplate = sectionTemplate };
            Header = templateEditor;
            sectionTemplate.Articles.CollectionChanged += Articles_CollectionChanged;
        }

        private void Articles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                // faire la liste des treeviewitem à supprimer
                var collectionForRemoval = from sti in Items.OfType<ArticleTemplateItem>() where e.OldItems.Contains(sti.ArticleTemplate) select sti;
                List<ArticleTemplateItem> listForRemoval = collectionForRemoval.ToList();
                listForRemoval.ForEach(toRemove => Items.Remove(toRemove));
            }

            if (e.NewItems != null)
            {
                foreach (ArticleTemplate at in e.NewItems)
                {
                    ArticleTemplateItem ati = new ArticleTemplateItem(at);
                    Items.Add(ati);
                }
            }
        }

        public SectionTemplate SectionTemplate
        {
            get { return sectionTemplate; }
        }

    }
}
