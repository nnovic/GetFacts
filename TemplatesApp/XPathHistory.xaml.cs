using GetFacts.Parse;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logique d'interaction pour XPathHistory.xaml
    /// </summary>
    public partial class XPathHistory : UserControl
    {
        private readonly ObservableCollection<AbstractXPathBuilder> History = new ObservableCollection<AbstractXPathBuilder>();

        public XPathHistory()
        {
            InitializeComponent();
            this.DataContext = History;
        }

        public void Add(AbstractXPathBuilder xpath)
        {
            if (History.Contains(xpath) == false)
            {
                History.Insert(0, xpath);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            History.Clear();
        }
    }
}
