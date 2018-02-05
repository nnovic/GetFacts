using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace GetFacts
{
    /// <summary>
    /// Logique d'interaction pour BackgroundControl.xaml
    /// </summary>
    public partial class BackgroundControl : UserControl
    {
        private readonly Random random = new Random();

        private readonly DispatcherTimer timer = new DispatcherTimer();
        private Color bottomBgColor;
        private Color bottomFgColor;
        private Color topBgColor;
        private Color topFgColor;

        public BackgroundControl()
        {
            RandomizeColors();
            InitializeComponent();
        }

        private Color RandomColor()
        {
            double r = random.NextDouble() * 200.0 + 55.0;
            double g = random.NextDouble() * 200.0 + 55.0;
            double b = random.NextDouble() * 200.0 + 55.0;
            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }

        public void RandomizeColors()
        {
            bottomBgColor = RandomColor();
            bottomFgColor = Lighter(bottomBgColor);

            topBgColor = RandomColor();
            topFgColor = Darker(topBgColor);
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            timer.Tick += new EventHandler(DispatcherTimer_Tick);
            timer.Interval = TimeSpan.FromSeconds(0.1);
            GenerateShapes();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            AnimationEnabled = true;
        }

        private Point[,] percentageMatrix=null;
        private Point[,] sizedMatrix=null;
        private Point[,] animatedMatrix = null;
        private int rowCount = 10;
        private int colCount = 10;

        private void GenerateShapes()
        {
            rowCount = 5 + random.Next(15);
            rowCount = colCount = 5 + random.Next(15);

            // génération de la matrice de points,
            // en pourcentage de la taille réelle du control.
            percentageMatrix = GeneratePoints(rowCount, colCount);
            sizedMatrix = new Point[rowCount, colCount];
            animatedMatrix =  new Point[rowCount, colCount]; 
            ShufflePoints(percentageMatrix, rowCount, colCount);
        }

        private void RescaleShapes()
        {
            RescalePoints(percentageMatrix, sizedMatrix, rowCount, colCount);
            RescaleAnimation(sizedMatrix, animatedMatrix, rowCount, colCount);
        }
        
        private void RedrawShapes()
        {
            Canvas.BeginInit();
            try
            {
                Canvas.Children.Clear();
                List<Shape> shapes = GeneratePolygons(animatedMatrix, rowCount, colCount);
                shapes.ForEach(s => Canvas.Children.Add(s));
            }
            finally
            {
                Canvas.EndInit();
            }
        }

        private Point[,] GeneratePoints(int rows, int columns)
        {
            Point[,] output = new Point[rows,columns];
            double width = ActualWidth;
            double height = ActualHeight;

            for(int row=0; row<rows; row++)
            {
                for(int col=0; col<columns; col++)
                {

                    Point p = new Point();
                    p.X = 1.0 * col / (columns - 1);
                    p.Y = 1.0 * row / (rows - 1);
                    output[row, col] = p;
                }
            }

            return output;
        }

        private void RescalePoints(Point[,] originalMatrix, Point[,] actualMatrix, int rows, int columns)
        {
            double width = ActualWidth;
            double height = ActualHeight;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    actualMatrix[row,col].X = width * originalMatrix[row, col].X;
                    actualMatrix[row, col].Y = height * originalMatrix[row, col].Y;
                }
            }
        }

        private void ShufflePoints(Point[,] points, int rows, int columns)
        {
            double width = 1.0;
            double height = 1.0;

            double hMaxJitter = (width / (columns - 1)) / 4;
            double vMaxJitter = (height / (rows - 1)) / 4;

            for (int row = 1; row < (rows-1); row++)
            {
                for (int col =1; col < (columns-1); col++)
                {
                    double hJitter = (-hMaxJitter) + random.NextDouble() * (2 * hMaxJitter);
                    double vJitter = (-vMaxJitter) + random.NextDouble() * (2 * vMaxJitter);
                    points[row, col].X += hJitter;
                    points[row, col].Y += vJitter;
                }
            }
        }

        private List<Shape> GeneratePolygons(Point[,] points, int rows, int columns)
        {
            List<Shape> output = new List<Shape>();

            for(int row=0; row<(rows-1); row++)
            {
                for(int col=0; col<(columns-1); col++)
                {
                    Polygon p = new Polygon();
                    PointCollection pc = new PointCollection();
                    pc.Add(points[row,col]);
                    pc.Add(points[row, col+1]);
                    pc.Add(points[row+1, col + 1]);
                    pc.Add(points[row+1, col]);
                    p.Points = pc;

                    p.Stroke = PickForegroundColor(row, col, rows, columns);
                    p.Fill = PickBackgroundColor(row, col, rows, columns);
                    p.StrokeThickness = 2;

                    output.Add(p);
                }
            }

            return output;
        }

        private Brush PickForegroundColor(int row, int col, int maxRows, int maxColumns)
        {
            double horizontally = (double)col / (double)(maxColumns - 1);
            double vertically =  1.0-(double)row / (double)(maxRows - 1);
            return PickColor(vertically, horizontally,bottomFgColor,topFgColor);
        }

        private Brush PickBackgroundColor(int row, int col, int maxRows, int maxColumns)
        {
            double horizontally = (double)col / (double)(maxColumns - 1);
            double vertically = 1.0 - (double)row / (double)(maxRows - 1);
            return PickColor(vertically, horizontally,bottomBgColor, topBgColor);
        }

        private Brush PickColor(double vScale, double hScale, Color bottomColor, Color topColor)
        {
            byte r = ComputeR(vScale, hScale, bottomColor, topColor);
            byte g = ComputeG(vScale, hScale, bottomColor, topColor);
            byte b = ComputeB(vScale, hScale, bottomColor, topColor);
            Color color = Color.FromRgb(r, g, b);
            return new SolidColorBrush(color);
        }

        private byte ComputeR(double vScale, double hScale, Color bottomColor, Color topColor)
        {
            return ComputeColorComponent(vScale, hScale, bottomColor.R, topColor.R);
        }

        private byte ComputeG(double vScale, double hScale, Color bottomColor, Color topColor)
        {
            return ComputeColorComponent(vScale, hScale, bottomColor.G, topColor.G);
        }
        private byte ComputeB(double vScale, double hScale, Color bottomColor, Color topColor)
        {
            return ComputeColorComponent(vScale, hScale, bottomColor.B, topColor.B);
        }

        private byte ComputeColorComponent(double vScale, double hScale, byte vMin, byte vMax)
        {
            double vValue = (double)vMin + vScale * (double)(vMax - vMin);
            return (byte)Math.Round(vValue);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RescaleShapes();
            RedrawShapes();
        }

        private Color Lighter(Color c)
        {
            const int incr = 20;
            byte r = (byte)Math.Min(255, c.R + incr);
            byte g = (byte)Math.Min(255, c.G + incr);
            byte b = (byte)Math.Min(255, c.B + incr);
            return Color.FromRgb(r, g, b);
        }

        private Color Darker(Color c)
        {
            const int decr = 20;
            byte r = (byte)Math.Max(0, c.R - decr);
            byte g = (byte)Math.Max(0, c.G - decr);
            byte b = (byte)Math.Max(0, c.B - decr);
            return Color.FromRgb(r, g, b);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }


        private int animationStep = 0;
        private int animationCycle = 10;
        private double[] animationXIncrements;
        private double[] animationYIncrements;


        private void RescaleAnimation(Point[,] inputMatrix, Point[,] outputMatrix, int rows, int columns)
        {
            animationXIncrements = new double[2 * animationCycle];
            animationYIncrements = new double[2 * animationCycle];

            for (int keyframe = 0; keyframe < animationCycle; keyframe++)
            {
                animationXIncrements[keyframe] = 3 * Math.Cos(2 * Math.PI * keyframe / animationCycle);
                animationYIncrements[keyframe] = 3 * Math.Cos(2 * Math.PI * keyframe / animationCycle);

                animationXIncrements[animationCycle + keyframe] = animationXIncrements[keyframe];
                animationYIncrements[animationCycle + keyframe] = animationYIncrements[keyframe];
            }


            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    outputMatrix[row, col] = new Point(inputMatrix[row, col].X, inputMatrix[row, col].Y);
                }
            }
        }

        private void AnimateShapes(Point[,] inputMatrix, Point[,] outputMatrix, int rows, int columns)
        {
            rows--;
            columns--;

            for (int row = 1; row < rows; row++)
            {
                for (int col = 1; col < columns; col++)
                {                    
                    outputMatrix[row, col] = inputMatrix[row, col];
                    int decalage = (row + col) % animationCycle;
                    outputMatrix[row, col].Offset(animationXIncrements[animationStep+decalage], animationYIncrements[animationStep+decalage]);
                }
            }
        }


        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            animationStep++;
            if(animationStep>=animationCycle)
            {
                animationStep = 0;
            }
            AnimateShapes(sizedMatrix, animatedMatrix, rowCount, colCount);
            RedrawShapes();
            InvalidateVisual();
        }


        public bool AnimationEnabled
        {
            set
            {
                if((value==true) && (DesignerProperties.GetIsInDesignMode(this)==false) )
                {
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                }
            }
        }
    }
}
