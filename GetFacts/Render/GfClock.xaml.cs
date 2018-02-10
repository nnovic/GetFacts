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
using System.Windows.Threading;

namespace GetFacts.Render
{
    /// <summary>
    /// Logique d'interaction pour GfClock.xaml
    /// </summary>
    public partial class GfClock : UserControl
    {
        private readonly DispatcherTimer clockTimer;

        public GfClock()
        {
            InitializeComponent();

            clockTimer = new DispatcherTimer();
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Interval = TimeSpan.FromSeconds(1);
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            const double Thickness = 2.0;
            Brush GrayBrush = new SolidColorBrush(Rendering.Default.ClockFrameColor);
            DateTime current = DateTime.Now;
            TimeOfDay.Content = current.ToShortTimeString();

            double circleDiameter = Math.Min(Canvas.ActualWidth, Canvas.ActualHeight);
            double circleRadius = circleDiameter / 2.0;
            double circleCenterX = Canvas.ActualWidth / 2.0;
            double circleCenterY = Canvas.ActualHeight / 2.0;
            double circleOffsetX = circleCenterX - circleRadius;
            double circleOffsetY = circleCenterY - circleRadius;

            double dotDiameter = Math.Max(2*Thickness, circleDiameter / 10.0);
            double dotRadius = dotDiameter / 2.0;
            double angle = 2 * Math.PI / 60.0 * (double)current.Second;
            double dotX = circleRadius * Math.Cos(angle);
            double dotY = circleRadius * Math.Sin(angle);

            Canvas.BeginInit();
            try
            {
                Canvas.Children.Clear();
                Ellipse e1 = new Ellipse
                {
                    Stroke = GrayBrush,
                    StrokeThickness = Thickness,
                    Width=circleDiameter,
                    Height= circleDiameter,
                };

                Ellipse e2 = new Ellipse
                {
                    Fill = GrayBrush,
                    Width = dotDiameter,
                    Height = dotDiameter
                };

                Canvas.Children.Add(e1);
                Canvas.SetLeft(e1, circleOffsetX);
                Canvas.SetTop(e1, circleOffsetY);

                Canvas.Children.Add(e2);
                Canvas.SetLeft(e2, (circleCenterX- dotRadius) + dotX);
                Canvas.SetTop(e2, (circleCenterY- dotRadius )+ dotY);
            }
            finally
            {
                Canvas.EndInit();
            }

            InvalidateVisual();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateClock();
            clockTimer.Start();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            clockTimer.Stop();
        }

        internal void Dispose()
        {
            clockTimer.Stop();
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateClock();
        }
    }
}
