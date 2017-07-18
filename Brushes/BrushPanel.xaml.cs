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
using System.Reflection;
using System.Collections;

namespace Brushes
{
    /// <summary>
    /// Logique d'interaction pour BrushPanel.xaml
    /// </summary>
    public partial class BrushPanel : UserControl
    {
        public BrushPanel()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            BrushSelection.Visibility = Visibility.Hidden;
            Hashtable brushes = GetBrushes();
            foreach(string name in brushes.Keys)
            {
                BrushSelection.Items.Add(name);
            }
        }


        #region get brushes

        private static readonly object _lock_ = new object();
        private static Hashtable _brushes = null;

        private Hashtable GetBrushes()
        {
            lock (_lock_)
            {
                if (_brushes == null)
                {
                    FindBrushes();
                }

                return Hashtable.Synchronized(_brushes);
            }
        }

        private static void FindBrushes()
        {
            _brushes = new Hashtable();

            Type t = typeof(System.Windows.Media.Brushes);
            MemberInfo[] members = t.FindMembers(MemberTypes.Property, BindingFlags.Public | BindingFlags.Static, new MemberFilter(DelegateToSearchCriteria),
                "ReferenceEquals");

            foreach( MemberInfo member in members )
            {
                if (member is PropertyInfo property)
                {
                    object value = property.GetValue(null);
                    _brushes.Add(property.Name, value);
                }
            }
        }

        public static bool DelegateToSearchCriteria(MemberInfo objMemberInfo, Object objSearch)
        {
            return true;
        }

        #endregion

        private void BrushName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BrushSelection.Visibility = Visibility.Visible;
        }

        private void BrushSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = (string)BrushSelection.SelectedItem;
            Hashtable brushes = GetBrushes();
            Brush brush = (Brush)brushes[name];
            Background = brush;
            BrushName.Content = name;
        }
    }
}
