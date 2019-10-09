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
		// Program state
		private Mode m_mode = Mode.Default;
		private DefaultMode m_defaultMode = DefaultMode.ArbitraryOrder;
		private IList<SupportingPoint> m_points = new List<SupportingPoint>();
		private BezierCurve m_curve;
		private bool m_isCurveDrawn = false;

		// Currently moving point
		double m_pointX;
		double m_pointY;
		double m_nextX;
		double m_nextY;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (m_isCurveDrawn)
			{
				return;
			}
			var supportingPoint = new SupportingPoint(e.GetPosition(Canvas));
			m_points.Add(supportingPoint);	
			Canvas.Children.Add(supportingPoint);			
		}

		private void ClearCanvas_Click(object sender, RoutedEventArgs e)
		{
			ResetDrawingSpace();
			m_isCurveDrawn = false;
		}

		private void ResetDrawingSpace()
		{
			Canvas.Children.Clear();
			m_points = new List<SupportingPoint>();
		}

		private void DrawButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_points.Count < 2)
			{
				return;
			}

			switch (m_mode)
			{
				case Mode.Default:
				{
					m_curve = new BezierCurve(m_points);
					foreach(var point in m_points)
					{
						Canvas.Children.Remove(point);
					}
					m_points = new List<SupportingPoint>();
					m_curve.Draw(Canvas);
					m_isCurveDrawn = true;
					CloseCurveButton.IsEnabled = true;
				}
				break;
			}
		}

		private void CloseCurveButton_Click(object sender, RoutedEventArgs e)
		{
			if (m_curve != null)
			{
				m_curve.Erase(Canvas);
				m_curve.Close();
				m_curve.Draw(Canvas);
			}			
		}
	}
}
