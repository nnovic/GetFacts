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
    /// Logique d'interaction pour NotificationItem.xaml
    /// </summary>
    public partial class NotificationItem : UserControl
    {
        public NotificationItem()
        {
            InitializeComponent();
        }

        public NotificationSystem.Notification Notification
        {
            set
            {
                DateAndTime.Text = value.Timestamp.ToString();
                Title.Text = value.Title;
                Description.Text = value.Description;
            }
        }
    }
}
