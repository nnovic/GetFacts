using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TemplatesApp
{
    class PageTemplateItem:TreeViewItem
    {
        private readonly PageTemplate pageTemplate;
        private readonly PageTemplateEditor templateEditor;

        public PageTemplateItem(PageTemplate template)
        {
            this.pageTemplate = template;

            templateEditor = new PageTemplateEditor() { PageTemplate = pageTemplate };
            Header = templateEditor;
            pageTemplate.Sections.CollectionChanged += Sections_CollectionChanged;
        }

        private void Sections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                // faire la liste des treeviewitem à supprimer
                var collectionForRemoval = from sti in Items.OfType<SectionTemplateItem>() where e.OldItems.Contains(sti.SectionTemplate) select sti;
                List<SectionTemplateItem> listForRemoval = collectionForRemoval.ToList();
                listForRemoval.ForEach(toRemove => Items.Remove(toRemove));
            }

            if (e.NewItems != null)
            {
                foreach (SectionTemplate st in e.NewItems)
                {
                    SectionTemplateItem ste = new SectionTemplateItem(st);
                    Items.Add(ste);
                }
            }
        }


        internal PageTemplate PageTemplate
        {
            get { return pageTemplate; }
        }

        /*private void TemplateEditor_SectionAdded(object sender, PageTemplateEditor.SectionAddedEventArgs e)
        {
            SectionTemplateItem ste = new SectionTemplateItem(e.SectionTemplate);
            Items.Add(ste);
        }*/
    }
}
