using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Logique d'interaction pour NotificationsPanel.xaml
    /// </summary>
    public partial class NotificationsPanel : UserControl
    {
        public NotificationsPanel()
        {
            InitializeComponent();
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {            
            NotificationSystem.GetInstance().Notifications.CollectionChanged += Notifications_CollectionChanged;
            UpdateList();
        }

        private void Root_Unloaded(object sender, RoutedEventArgs e)
        {
            NotificationSystem.GetInstance().Notifications.CollectionChanged -= Notifications_CollectionChanged;
        }

        private void Notifications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void UpdateList()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                _UpdateList();
            }), null);
        }

        private void _UpdateList()
        {
            ListOfNotifications.BeginInit();

            try
            {
                // dresser la liste des notifications actuellement
                // affichées dans la listbox.

                var currentlyKnownNotifications = 
                    from NotificationItem item 
                    in ListOfNotifications.Items
                    select item.Tag;

                // trouver toutes les notifications dans la
                // listbox qui n'existent plus dans NotificationSystem, 
                // afin de supprimer les NotificationItems correspondants

                var outdatedNotifications =
                    from notification
                    in currentlyKnownNotifications
                    where NotificationSystem.GetInstance().Notifications.Contains(notification) == false
                    select notification;

                var itemsToRemove = from NotificationItem item in ListOfNotifications.Items
                            where outdatedNotifications.Contains(item.Tag)
                            select item;

                List<NotificationItem> toRemove = itemsToRemove.ToList();
                toRemove.ForEach((item) => { ListOfNotifications.Items.Remove(item); });

                // trouver toutes les notifications dans NotificationSystem
                // qui n'existent pas dans la listbox

                var newNotifications =
                    from notification
                    in NotificationSystem.GetInstance().Notifications
                    where currentlyKnownNotifications.Contains(notification) == false
                    select notification;

                List<NotificationSystem.Notification> notificationsToAdd = newNotifications.ToList();
                notificationsToAdd.ForEach((notification) => { ListOfNotifications.Items.Add( new NotificationItem() { Notification=notification } ); });
                
            }
            finally
            {
                ListOfNotifications.EndInit();
            }
        }

        
    }

    public class HasItemsToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasItems = (bool)value;
            if (hasItems) return Visibility.Hidden;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return true;
        }
    }
}
