using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace GetFacts.Ads
{
    /// <summary>
    /// Logique d'interaction pour WindowsAPICodePack_Ad.xaml
    /// </summary>
    public partial class WindowsAPICodePack_Ad : UserControl
    {
        public static readonly string URL = "https://github.com/aybe/Windows-API-Code-Pack-1.1";

        public WindowsAPICodePack_Ad()
        {
            InitializeComponent();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(URL);
        }
    }
}
