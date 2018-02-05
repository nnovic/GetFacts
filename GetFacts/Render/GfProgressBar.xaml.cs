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

namespace GetFacts.Render
{
    /// <summary>
    /// Logique d'interaction pour GfProgressBar.xaml
    /// </summary>
    public partial class GfProgressBar : UserControl
    {
        private double maximum = 100;
        private double minimum = 0;
        private double value = 0;

        public GfProgressBar()
        {
            InitializeComponent();
        }

        public double Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                maximum = value;
                UpdateProgress();
            }
        }

        public double Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
                UpdateProgress();
            }
        }

        public double Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            double totalWidth = BackgroundRectangle.ActualWidth - ProgressRectangle.Margin.Left - ProgressRectangle.Margin.Right;
            double ratio = Value / (Maximum - Minimum);
            totalWidth *= ratio;
            ProgressRectangle.Width = totalWidth;
        }
    }
}
