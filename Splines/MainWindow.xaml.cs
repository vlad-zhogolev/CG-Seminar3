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

namespace Spline
{
	public static class Constants
	{
		public const int PIXEL_RADIUS = 1;
		public const int POINT_RADIUS = 2;
		public const int POINT_DIAMETER = 2 * POINT_RADIUS;
		public static readonly Brush POINT_COLOR = Brushes.Black;
		public static readonly Brush PIXEL_COLOR = Brushes.Blue;
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<SupportingPoint> m_points = new List<SupportingPoint>();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{			
			var point = new SupportingPoint(e.GetPosition(MainCanvas));
			m_points.Add(point);
			MainCanvas.Children.Add(point.Ellipse);
			//MainCanvas.Children.Clear();
			//DrawAllPoints();
		}

		private void ClearCanvas_Click(object sender, RoutedEventArgs e)
		{
			MainCanvas.Children.Clear();
			m_points.Clear();
		}

		private void DrawAllPoints()
		{
			foreach(var point in m_points)
			{
				Canvas.SetLeft(point, point.Coordinates.X);
				Canvas.SetTop(point, point.Coordinates.Y);
				MainCanvas.Children.Add(point);
			}
		}
		

		private void DrawPixel(Point point)
		{
			DrawPixel(point.X, point.Y);
		}

		private void DrawPixel(double x, double y)
		{
			var ellipse = new Ellipse();
			Canvas.SetLeft(ellipse, x);
			Canvas.SetTop(ellipse, y);
			ellipse.Width = Constants.PIXEL_RADIUS;
			ellipse.Height = Constants.PIXEL_RADIUS;
			ellipse.Fill = Constants.PIXEL_COLOR;

			MainCanvas.Children.Add(ellipse);
		}

		private void DrawPoint(Point point, int radius = Constants.POINT_RADIUS)
		{
			var ellipse = new Ellipse();			
			Canvas.SetLeft(ellipse, point.X - Constants.POINT_RADIUS);
			Canvas.SetTop(ellipse, point.Y - Constants.POINT_RADIUS);
			ellipse.Width = 2 * Constants.POINT_RADIUS;
			ellipse.Height = 2 * Constants.POINT_RADIUS;
			ellipse.Fill = Constants.POINT_COLOR;

			MainCanvas.Children.Add(ellipse);
		}
	}
}
