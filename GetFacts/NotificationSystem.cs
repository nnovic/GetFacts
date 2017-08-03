using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts
{
    /// <summary>
    /// Fournit un service de gestion de notifications,
    /// principalement destinées à prévenir l'utilisateur
    /// de problèmes durant l'utilisation de GetFactsApp.
    /// </summary>
    /// <remarks>Une classe qui ajoute des notifications devrait veiller à appeler 
    /// NotificationSystem.GetInstance().RemoveAll() dans son destructeur.</remarks>
    public class NotificationSystem
    {

        #region singleton

        private static NotificationSystem uniqueInstance = null;
        private readonly static object _lock_ = new object();

        public static NotificationSystem GetInstance()
        {
            lock(_lock_)
            {
                if(uniqueInstance==null)
                {
                    uniqueInstance = new NotificationSystem();
                }
                return uniqueInstance;
            }
        }

        private NotificationSystem()
        {

        }

        #endregion


        public class Notification:IEquatable<Notification>
        {
            public Notification(object source, int key)
            {
                Source = source;
                Key = key;
                Timestamp = DateTime.Now;
            }

            public object Source
            {
                get;
                private set;
            }

            public int Key
            {
                get;
                private set;
            }

            public string Title
            {
                get;set;
            }

            public string Description
            {
                get;set;
            }

            /// <summary>
            /// Définit dans quelles circonstances deux instances
            /// de Notification sont considérées comme égales.
            /// En l'occurence, deux instances de Notification sont
            /// égales si et seulement si leurs propriétés Source
            /// et Key sont égales entres elles.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            bool IEquatable<Notification>.Equals(Notification other)
            {
                if (other == null)
                    return false;

                if (Source != other.Source)
                    return false;

                if (Key != other.Key)
                    return false;

                return true;
            }

            public DateTime Timestamp
            {
                get;
                private set;
            }
        }

        public readonly ObservableCollection<Notification> Notifications = new ObservableCollection<Notification>();


        public void Add(Notification n)
        {
            lock(_lock_)
            {
                if(Notifications.Contains(n)==false)
                {
                    Notifications.Add(n);
                }
            }
        }


        public void Remove(Notification n)
        {
            lock(_lock_)
            {
                if(Notifications.Contains(n)==true)
                {
                    Notifications.Remove(n);
                }
            }
        }


        /// <summary>
        /// Supprimer toutes les notifications
        /// dont la propriété Source est la même référence que
        /// l'objet "source" passé en paramètre.
        /// </summary>
        /// <param name="source"></param>
        public void RemoveAll(object source)
        {
            lock(_lock_)
            {
                var notificationsFromThatSource = from n in Notifications where n.Source == source select n;
                List<Notification> notificationsToBeRemoved = notificationsFromThatSource.ToList();
                foreach(Notification n in notificationsToBeRemoved)
                {
                    Notifications.Remove(n);
                }
            }
        }
    }
}
