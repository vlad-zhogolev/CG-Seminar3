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
		private CursorMode m_cursorMode = CursorMode.SettingPoints;
		private DrawingMode m_drawingMode = DrawingMode.Default;
		private BezierLineType m_lineType = BezierLineType.ArbitraryOrder;
		private IList<SupportingPoint> m_points = new List<SupportingPoint>();
		private BezierCurve m_curve;
		private bool m_isCurveDrawn = false;

		// Currently moving point
		UIElement m_capturedPoint = null;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (m_cursorMode == CursorMode.MovingPoints)
			{
				return;
			}

			if ( m_isCurveDrawn )
			{
				return;
			}
			var supportingPoint = new SupportingPoint(e.GetPosition(Canvas));
			supportingPoint.MouseLeftButtonDown += PointStartMoving;
			supportingPoint.MouseMove += PointMoving;
			supportingPoint.MouseLeftButtonUp += PointEndMoving;
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

			switch (m_drawingMode)
			{
				case DrawingMode.Default:
				{
					m_curve = new BezierCurve(m_points, Canvas);
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
			if (m_curve != null && m_isCurveDrawn)
			{
				m_curve.Erase(Canvas);
				m_curve.Close();
				m_curve.Draw(Canvas);
			}			
		}

		private void OpenCurveButton_Click(object sender, RoutedEventArgs e)
		{
			if ( m_curve != null && m_isCurveDrawn )
			{
				m_curve.Erase(Canvas);
				m_curve.Open();
				m_curve.Draw(Canvas);
			}
		}

		private void PointStartMoving(object sender, MouseButtonEventArgs e)
		{
			if ( m_cursorMode != CursorMode.MovingPoints )
			{
				return;
			}
			(sender as SupportingPoint).Opacity = 0.5;
			m_capturedPoint = (UIElement)sender;
			Mouse.Capture(m_capturedPoint);
		}

		private void PointMoving(object sender, MouseEventArgs e)
		{
			if (m_capturedPoint != null)
			{
				var point = sender as SupportingPoint;
				double x = e.GetPosition(Canvas).X;
				double y = e.GetPosition(Canvas).Y;
				if ( x < 0 || y < 0 || x > Canvas.ActualWidth || y > Canvas.ActualHeight )
					return;
				point.X += x - point.X;
				point.Y += y - point.Y;
			}
		}

		private void PointEndMoving(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			(sender as SupportingPoint).Opacity = 1;
			Mouse.Capture(null);
			m_capturedPoint = null;
		}

		private void MovePointsRadioButton_Checked(object sender, RoutedEventArgs e)
		{
			m_cursorMode = CursorMode.MovingPoints;
			if ( SetPointsRadioButton != null )
			{
				SetPointsRadioButton.IsChecked = false;
			}
		}

		private void SetPointsRadioButton_Checked(object sender, RoutedEventArgs e)
		{
			m_cursorMode = CursorMode.SettingPoints;
			if ( MovePointsRadioButton != null )
			{
				MovePointsRadioButton.IsChecked = false;
			}
		}
	}
}
