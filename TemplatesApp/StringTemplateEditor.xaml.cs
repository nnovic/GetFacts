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
        public static readonly System.Windows.DependencyProperty SuggestionsKeyProperty;

        static StringTemplateEditor()
        {
            SuggestionsKeyProperty = System.Windows.DependencyProperty.Register(
                "SuggestionsKey",
                typeof(string),
                typeof(StringTemplateEditor),
                new FrameworkPropertyMetadata(OnSuggestionsKeyChanged)
                );

        }

        static void OnSuggestionsKeyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            StringTemplateEditor self = (StringTemplateEditor)sender;
            self.SuggestionsButton.Visibility = string.IsNullOrEmpty(self.SuggestionsKey) ? Visibility.Collapsed : Visibility.Visible;
        }


        public StringTemplateEditor()
        {
            InitializeComponent();
        }

        #region SuggestionsKey

        //private string _suggestionsKey = null;
        private ContextMenu suggestionsMenu = null;

        public string SuggestionsKey
        {
            get
            {
                return (string)GetValue(StringTemplateEditor.SuggestionsKeyProperty);
            }
            set
            {
                SetValue(StringTemplateEditor.SuggestionsKeyProperty, value);
            }
        }

        ContextMenu SuggestionsMenu
        {
            get
            {
                if( suggestionsMenu==null )
                {
                    string key = SuggestionsKey;
                    if (string.IsNullOrEmpty(key))
                        return null;

                    string json = Properties.Resources.ResourceManager.GetString(key);
                    if (string.IsNullOrEmpty(json))
                        return null;

                    List<string> suggestions;

                    try
                    {
                        suggestions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
                        if ((suggestions == null) || (suggestions.Count == 0))
                            return null;
                    }
                    catch
                    {
                        return null;
                    }


                    suggestionsMenu = this.FindResource("suggestionsMenu") as ContextMenu;
                    foreach (string suggestion in suggestions)
                    {
                        MenuItem item = new MenuItem()
                        {
                            Header = suggestion,
                        };
                        item.Click += Item_Click;
                        suggestionsMenu.Items.Add(item);
                    }
                }
                return suggestionsMenu;
            }
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string suggestion = (string)item.Header;
            XPathInput.Text = suggestion;
        }

        #endregion

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

        private void XPathInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(XPathInput.IsValid )
                StringTemplate.XPath = XPathInput.Text;
        }

        private void RegexInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RegexInput.IsValid)
                StringTemplate.Regex = RegexInput.Text;
        }

        private void Suggestions_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = SuggestionsMenu;
            if (cm != null)
            {
                cm.PlacementTarget = SuggestionsButton;
                cm.IsOpen = true;
            }
        }
    }
}
