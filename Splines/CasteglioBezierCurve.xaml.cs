using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Spline
{
	public class Line
	{
		public Point Begin { get; set; }
		public Point End { get; set; }

		public Line(Point begin, Point end)
		{
			Begin = begin;
			End = end;
		}

		public Point Interpolate(double t)
		{		
			var x = (1.0 - t) * Begin.X + t * End.Y;
			var y = (1.0 - t) * Begin.Y + t * End.Y;
			return new Point(x, y);
		}
	}

	/// <summary>
	/// Interaction logic for CasteglioBezierCurve.xaml
	/// </summary>
	public partial class CasteglioBezierCurve : UserControl, IBezierCurve
	{
		public static readonly double STEP = 0.01;
		private IList<SupportingPoint> m_supportingPoints;
		private IList<Point> m_points = new List<Point>();
		private bool m_isClosed;
		private Canvas m_canvas;

		public CasteglioBezierCurve(IList<SupportingPoint> points, Canvas canvas, bool isClosed = false)
		{
			InitializeComponent();
			m_canvas = canvas;
			m_isClosed = isClosed;
			m_supportingPoints = points;
			foreach ( var point in m_supportingPoints )
			{
				point.PropertyChanged += PointPositionChanged;
			}
			CalculateCurve();
		}

		public void Draw(Canvas canvas)
		{
			canvas.Children.Add(this);
		}

		public void Erase(Canvas canvas)
		{
			canvas.Children.Remove(this);
		}

		public void Open()
		{
			if ( m_isClosed )
			{
				OpenCurve();
				CalculateCurve();
				m_isClosed = false;
			}
		}

		public void Close()
		{
			if (!m_isClosed)
			{
				CloseCurve();
				CalculateCurve();
				m_isClosed = true;
			}
		}

		private void CloseCurve()
		{
			//var point = CalculateSymmetricPoint(m_supportingPoints[0], m_supportingPoints[1]);
			//point.PropertyChanged += PointPositionChanged;
			//m_supportingPoints.Add(new Point(point.X, point.Y));
			//m_supportingPoints.Add(m_points[0]);
		}

		private void OpenCurve()
		{
			//m_points.RemoveAt(m_points.Count - 1);
			//m_points.RemoveAt(m_points.Count - 1);
		}

		private void CalculateCurve()
		{
			Curve.Points.Clear();
			IList<Point> result = new List<Point>();

			foreach ( var point in m_supportingPoints )
			{
				result.Add(new Point(point.X, point.Y));
			}
			for ( double t = 0.0 ; t <= 1.0 + STEP ; t += STEP )
			{
				var point = RecursiveCasteglio(t, result);
				Curve.Points.Add(point);
			}
		}

		private static Point RecursiveCasteglio(double t, IList<Point> points)
		{			
			IList<Point> result = new List<Point>();
			for (int i = 0 ; i < points.Count - 1 ; ++i)
			{
				result.Add(Interpolate(points[i], points[i + 1], t));
			}
			if (result.Count == 1)
			{
				return result[0];
			}
			return RecursiveCasteglio(t, result);
		}

		private void PointPositionChanged(object sender, PropertyChangedEventArgs e)
		{
			Erase(m_canvas);
			CalculateCurve();
			Draw(m_canvas);
		}

		private static Point Interpolate(Point begin, Point end, double t)
		{
			var x = (1.0 - t) * begin.X + t * end.X;
			var y = (1.0 - t) * begin.Y + t * end.Y;
			return new Point(x, y);
		}

		private static SupportingPoint CalculateSymmetricPoint(SupportingPoint symmetryCenter, SupportingPoint point)
		{
			double x = symmetryCenter.X - (point.X - symmetryCenter.X);
			double y = symmetryCenter.Y - (point.Y - symmetryCenter.Y);
			return new SupportingPoint(x, y);
		}
	}
}
