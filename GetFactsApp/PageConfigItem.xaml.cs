using GetFacts.Facts;
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

namespace GetFacts
{
    /// <summary>
    /// Logique d'interaction pour PageConfigItem.xaml
    /// </summary>
    public partial class PageConfigItem : UserControl
    {
        private PageConfig pageConfig;

        public PageConfigItem()
        {
            InitializeComponent();
        }

        public PageConfig PageConfig
        {
            get
            {
                return pageConfig;
            }
            set
            {
                pageConfig = value;
                ConfigNameInput.Text = pageConfig.Name;
                ConfigUrlInput.Text = pageConfig.Url;
                ConfigTemplateInput.SelectedItem = pageConfig.Template;
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            foreach (string t in Templates)
            {
                ConfigTemplateInput.Items.Add(t);
            }
        }


        #region validity

        public bool IsValid
        {
            get
            {
                return IsNameValid && IsUrlValid && IsTemplateValid;
            }
        }

        bool IsNameValid
        {
            get
            {
                return !string.IsNullOrEmpty(ConfigNameInput.Text);
            }
        }

        bool IsUrlValid
        {
            get
            {
                return !string.IsNullOrEmpty(ConfigUrlInput.Text);
            }
        }

        bool IsTemplateValid
        {
            get
            {
                string sel = ConfigTemplateInput.SelectedItem as string;
                return sel != null;
            }
        }

        #endregion



        #region cached list of templates

        private static List<string> templates = null;
        private static readonly object _lock_ = new object();

        List<string> Templates
        {
            get
            {
                lock (_lock_)
                {
                    if (templates == null)
                    {
                        string dir = ConfigFactory.GetInstance().TemplatesDirectory;
                        templates = TemplateFactory.CreateTemplatesList(dir);
                    }
                    return templates;
                }
            }
        }


        #endregion

        private void DeleteThisItemButton_Click(object sender, RoutedEventArgs e)
        {
            ListView parent = (ListView)Parent;
            parent.Items.Remove(this);
        }

        private void ConfigNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsNameValid)
            {
                PageConfig.Name = ConfigNameInput.Text;
            }
        }

        private void ConfigUrlInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsUrlValid)
            {
                PageConfig.Url = ConfigUrlInput.Text;
            }
        }

        private void ConfigTemplateInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsTemplateValid)
            {
                PageConfig.Template = (string)ConfigTemplateInput.SelectedItem;
            }
        }
    }
}
