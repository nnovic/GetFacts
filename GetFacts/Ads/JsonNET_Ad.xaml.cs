using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace GetFacts.Ads
{
    /// <summary>
    /// Logique d'interaction pour Json.xaml
    /// </summary>
    public partial class JsonNET : UserControl
    {
        public static readonly string URL = "http://www.newtonsoft.com/json";

        public JsonNET()
        {
            InitializeComponent();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(URL);
        }
    }
}
