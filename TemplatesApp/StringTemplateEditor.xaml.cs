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
    /// Logique d'interaction pour StringTemplateEditor.xaml
    /// </summary>
    public partial class StringTemplateEditor : UserControl
    {
        public StringTemplateEditor()
        {
            InitializeComponent();
        }

        #region StringTemplate

        private StringTemplate stringTemplate = null;

        public StringTemplate StringTemplate
        {
            get
            {
                return stringTemplate;
            }
            set
            {
                ClearTemplate();
                stringTemplate = value;
                InitTemplate();
            }
        }

        private void ClearTemplate()
        {
        }

        private void InitTemplate()
        {
            if (StringTemplate != null)
            {
                XPathInput.Text = StringTemplate.XPath;
                RegexInput.Text = StringTemplate.Regex;
            }
        }

        #endregion
    }
}
