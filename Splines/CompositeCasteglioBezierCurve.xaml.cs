using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Spline
{

	/// <summary>
	/// Interaction logic for CompositeCasteglioBezierCurve.xaml
	/// </summary>
	public partial class CompositeCasteglioBezierCurve : UserControl, IBezierCurve
	{
		public static readonly double STEP = 0.05;
		private IList<SupportingPoint> m_supportingPoints;
		private IList<Edge> m_edges;
		private double[,] m_baseMatrix;
		private bool m_isClosed;
		private Canvas m_canvas;

		public CompositeCasteglioBezierCurve(IList<SupportingPoint> points, Canvas canvas, bool isClosed = false)
		{
			if ( points.Count < 6 )
			{
				throw new ArgumentException("Composite bezier curve must have at least  supporting points");
			}
			InitializeComponent();
			m_supportingPoints = points;
			m_isClosed = isClosed;
			m_canvas = canvas;
			m_edges = new List<Edge>();
			for ( int i = 0 ; i < m_supportingPoints.Count ; i += 3 )
			{
				m_edges.Add(new Edge(m_supportingPoints[i], m_supportingPoints[i + 1], m_supportingPoints[i + 2]));
				for ( int j = 0 ; j < 3 ; ++j )
				{
					m_supportingPoints[i + j].PropertyChanged += PointPositionChanged;
				}
				m_edges[m_edges.Count - 1].Points.Add(m_supportingPoints[i]);
			}
			if ( m_isClosed )
			{
				CloseCurve();
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

		}

		public void OpenCurve()
		{

		}

		public void Close()
		{


		}

		public void CloseCurve()
		{

		}

		private void CalculateCurve()
		{
			Curve.Points.Clear();
			IList<Point> result = new List<Point>();

			for ( int i = 0 ; i < m_edges.Count - 1 ; ++i )
			{
				for ( double t = 0.0 ; t <= 1.0 + STEP ; t += STEP )
				{
					var point = CalculateSegment(t, m_edges[i], m_edges[i + 1]);
					Curve.Points.Add(point);
				}
			}
		}

		private static Point CalculateSegment(double t, Edge first, Edge second)
		{
			IList<Point> result = new List<Point>();
			result.Add(new Point(first.Middle.X, first.Middle.Y));
			result.Add(new Point(first.End.X, first.End.Y));
			result.Add(new Point(second.Begin.X, second.Begin.Y));
			result.Add(new Point(second.Middle.X, second.Middle.Y));
			return RecursiveCasteglio(t, result);
		}

		private static Point RecursiveCasteglio(double t, IList<Point> points)
		{
			IList<Point> result = new List<Point>();
			for ( int i = 0 ; i < points.Count - 1 ; ++i )
			{
				result.Add(Interpolate(points[i], points[i + 1], t));
			}
			if ( result.Count == 1 )
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
