using GetFacts;
using GetFacts.Facts;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SetFactsApp
{
    /// <summary>
    /// Logique d'interaction pour PageConfigListItem.xaml
    /// </summary>
    public partial class PageConfigListItem : UserControl
    {
        private PageConfig pageConfig = null;

        public PageConfigListItem()
        {
            InitializeComponent();
        }

        public PageConfig PageConfig
        {
            get
            {
                return null;
            }
            set
            {
                pageConfig = value;
                NameInput.Text = pageConfig.Name;
                UrlInput.Text = pageConfig.Url;
                TemplateInput.Text = pageConfig.Template;
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            TemplateInput.ItemsSource = Templates;
        }

        #region cached list of templates

        private static List<string> templates = null;
        private static readonly object _lock_ = new object();



        static List<string> CreateTemplatesList()
        {
            List<string> output = new List<string>();
            string dir = ConfigFactory.GetInstance().TemplatesDirectory;
            foreach (string path in Directory.EnumerateFiles(dir,"*.json",SearchOption.AllDirectories))
            {
                string file = Toolkit.GetRelativePath(path, dir);
                output.Add(file);
            }
            return output;
        }

        List<string> Templates
        {
            get
            {
                lock(_lock_)
                {
                    if(templates==null)
                    {
                        templates = CreateTemplatesList();
                    }
                    return templates;
                }
            }
        }

        #endregion


    }
}
