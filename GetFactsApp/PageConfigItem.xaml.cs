using GetFacts.Facts;
using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                ConfigTemplateInput.Text = pageConfig.Template;
                TemplatesList.SearchPattern = null; ;
                EnabledCheckBox.IsChecked = pageConfig.Enabled;
                DownloadPeriodInput.Value = pageConfig.Refresh;
                IsNewBehaviorInput.SelectedValue = pageConfig.IsNewBehavior;
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
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
                string sel = ConfigTemplateInput.Text as string;
                return TemplatesList.Items.Contains(sel);
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

        private void EnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IsEnableChecked = true;
        }

        private void EnabledCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IsEnableChecked = false;
        }

        private bool IsEnableChecked
        {
            set
            {
                if(value)
                {
                    PageConfig.Enabled = true;
                    TheBorder.BorderBrush = Brushes.Black;
                    EnabledCheckBox.FontWeight = FontWeights.Bold;
                }
                else
                {
                    PageConfig.Enabled = false;
                    TheBorder.BorderBrush = Brushes.Silver;
                    EnabledCheckBox.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void IsNewBehaviorInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pageConfig.IsNewBehavior = (AbstractInfo.IsNewPropertyGets)IsNewBehaviorInput.SelectedItem;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            IsEnableChecked = (bool)EnabledCheckBox.IsChecked;
            TemplatesList.TemplatesDirectory = TemplateFactory.GetInstance().TemplatesDirectory;
        }

        private void TemplatesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string template = (string)e.AddedItems[0];
                ConfigTemplateInput.Text = template;
                TemplatesList.SearchPattern = null;
            }
        }

        private void ConfigTemplateInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TemplatesList.SearchPattern = ConfigTemplateInput.Text;
        }
    }
}
