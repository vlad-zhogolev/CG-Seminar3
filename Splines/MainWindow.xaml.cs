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
		private IBezierCurve m_curve;
		private IBezierCurve m_casteglioCurve;	
		private bool m_isCurveDrawn = false;

		// Currently moving point
		UIElement m_capturedPoint = null;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (m_cursorMode == CursorMode.MovingPoints || m_isCurveDrawn)
			{
				return;
			}

			switch (m_lineType)
			{
				case BezierLineType.ArbitraryOrder:
				{
					var supportingPoint = new SupportingPoint(e.GetPosition(Canvas));
					AddHandlers(supportingPoint);
					m_points.Add(supportingPoint);
					Canvas.Children.Add(supportingPoint);
				}
				break;
				case BezierLineType.ThirdOrder:
				{
					if ( m_points.Count == 4 )
					{
						return;
					}
					var supportingPoint = new SupportingPoint(e.GetPosition(Canvas));
					AddHandlers(supportingPoint);
					m_points.Add(supportingPoint);
					Canvas.Children.Add(supportingPoint);										
				}
				break;
				case BezierLineType.CompositeThirdOrder:
				{
					var supportingPoint = new SupportingPoint(e.GetPosition(Canvas));
					AddHandlers(supportingPoint);
					m_points.Add(supportingPoint);
					Canvas.Children.Add(supportingPoint);

					if (m_points.Count % 6 == 4 || (m_points.Count > 4 && m_points.Count % 6 == 1))
					{
						var size = m_points.Count;
						supportingPoint = Utility.CalculateSymmetricPoint(m_points[size - 1], m_points[size - 2]);
						AddHandlers(supportingPoint);
						m_points.Add(supportingPoint);
						Canvas.Children.Add(supportingPoint);
					}
				}
				break;
				default:
				{
					throw new Exception("Wrong BezierLineType");
				}
				break;
			}
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

			if (m_lineType == BezierLineType.CompositeThirdOrder && (m_points.Count < 4 || m_points.Count % 3 == 0))
			{
				return;
			}

			switch (m_drawingMode)
			{
				case DrawingMode.Default:
				{
					DrawCurve();
				}
				break;
				case DrawingMode.DefaultWithCastiglio:
				{
					DrawWithCastiglio();
				}
				break;
			}
		}

		private void DrawCurve()
		{
			switch ( m_lineType )
			{
				case BezierLineType.ArbitraryOrder:
				case BezierLineType.ThirdOrder:
				{
					m_curve = new BezierCurve(m_points, Canvas);
					foreach ( var point in m_points )
					{
						Canvas.Children.Remove(point);
					}
					m_points = new List<SupportingPoint>();
					m_curve.Draw(Canvas);
					m_isCurveDrawn = true;
					CloseCurveButton.IsEnabled = true;
				}
				break;
				case BezierLineType.CompositeThirdOrder:
				{
					var supportingPoint = Utility.CalculateSymmetricPoint(m_points[0], m_points[1]);
					AddHandlers(supportingPoint);
					m_points.Insert(0, supportingPoint);

					m_curve = new CompositeBezierCurve(m_points, Canvas);
					foreach ( var point in m_points )
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
		
		private void DrawWithCastiglio()
		{
			switch ( m_lineType )
			{
				case BezierLineType.ArbitraryOrder:
				case BezierLineType.ThirdOrder:
				{
					m_curve = new BezierCurve(m_points, Canvas);
					m_casteglioCurve = new CasteglioBezierCurve(m_points, Canvas);
					foreach ( var point in m_points )
					{
						Canvas.Children.Remove(point);
					}
					m_points = new List<SupportingPoint>();
					m_curve.Draw(Canvas);
					m_casteglioCurve.Draw(Canvas);
					m_isCurveDrawn = true;
					CloseCurveButton.IsEnabled = true;
				}
				break;
				case BezierLineType.CompositeThirdOrder:
				{
					var supportingPoint = Utility.CalculateSymmetricPoint(m_points[0], m_points[1]);
					AddHandlers(supportingPoint);
					m_points.Insert(0, supportingPoint);

					m_curve = new CompositeBezierCurve(m_points, Canvas);
					m_casteglioCurve = new CasteglioBezierCurve(m_points, Canvas);
					foreach ( var point in m_points )
					{
						Canvas.Children.Remove(point);
					}
					m_points = new List<SupportingPoint>();
					m_curve.Draw(Canvas);
					m_casteglioCurve.Draw(Canvas);
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

		private void AddHandlers(SupportingPoint supportingPoint)
		{
			supportingPoint.MouseLeftButtonDown += PointStartMoving;
			supportingPoint.MouseMove += PointMoving;
			supportingPoint.MouseLeftButtonUp += PointEndMoving;
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

		private void ArbitraryOrderCurveButton_Click(object sender, RoutedEventArgs e)
		{
			SwitchLineType(BezierLineType.ArbitraryOrder);
		}

		private void ThirdOrderCurveButton_Click(object sender, RoutedEventArgs e)
		{
			SwitchLineType(BezierLineType.ThirdOrder);
		}

		private void CompositeCurveButton_Click(object sender, RoutedEventArgs e)
		{
			SwitchLineType(BezierLineType.CompositeThirdOrder);
		}	
		
		private void SwitchLineType(BezierLineType type)
		{
			m_lineType = type;
			ResetDrawingSpace();
		}

		private void WithCasteglioRadioButton_Checked(object sender, RoutedEventArgs e)
		{
			m_drawingMode = DrawingMode.DefaultWithCastiglio;
			if (DefaultDrawingModeRadioButton != null)
			{
				DefaultDrawingModeRadioButton.IsChecked = false;
			}
		}

		private void DefaultDrawingModeRadioButton_Checked(object sender, RoutedEventArgs e)
		{
			m_drawingMode = DrawingMode.Default;
			if ( DefaultDrawingModeRadioButton != null )
			{
				WithCasteglioRadioButton.IsChecked = false;
			}
		}
	}
}
