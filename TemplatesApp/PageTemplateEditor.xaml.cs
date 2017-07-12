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
    /// Logique d'interaction pour PageTemplateEditor.xaml
    /// </summary>
    public partial class PageTemplateEditor : UserControl
    {
        public PageTemplateEditor()
        {
            InitializeComponent();
        }

        public PageTemplate PageTemplate
        {
            get;set;
        }

        private void AddSectionButton_Click(object sender, RoutedEventArgs e)
        {
            SectionTemplate newSection = new SectionTemplate();
            PageTemplate.Sections.Add(newSection);
        }
    }
}
