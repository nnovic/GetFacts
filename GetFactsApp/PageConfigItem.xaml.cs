using GetFacts.Facts;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
            DownloadPeriodInput.Minimum = PageConfig.MinRefresh;
            DownloadPeriodInput.Maximum = PageConfig.MaxRefresh;
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
                EnabledCheckBox.IsChecked = pageConfig.Enabled;
                DownloadPeriodInput.Value = pageConfig.Refresh;
                IsNewBehaviorInput.SelectedValue = pageConfig.IsNewBehavior;
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            foreach (string t in Templates)
            {
                ConfigTemplateInput.Items.Add(t);
            }

            IsNewBehaviorInput.ItemsSource = Enum.GetValues(typeof(AbstractInfo.IsNewPropertyGets));
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
                        string dir = TemplateFactory.GetInstance().TemplatesDirectory;
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


        /// <summary>
        /// Sur modif de la sélection dans la combobox
        /// "ConfigTemplateInput" (et à condition que
        /// IsTemplateValid retourne true) :
        /// - mettre à jour le PageConfig sous-jacent
        /// - mettre à jour l'URL courant avec l'URL de référence du nouveau template (si IsUrlValid retourne false, uniquement).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigTemplateInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsTemplateValid)
            {
                string path = (string)ConfigTemplateInput.SelectedItem;
                PageConfig.Template = path;
                PageTemplate pt = TemplateFactory.GetInstance().GetExistingTemplate(path);
                if(!IsUrlValid)
                {
                    ConfigUrlInput.Text = pt.Reference;
                }
            }
        }

        private void EnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PageConfig.Enabled = true;
        }

        private void EnabledCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PageConfig.Enabled = false;
        }

        private void IsNewBehaviorInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pageConfig.IsNewBehavior = (AbstractInfo.IsNewPropertyGets)IsNewBehaviorInput.SelectedItem;
        }
    }
}
