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
				m_points.Add(new Point(point.X, point.Y));
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

		public void Close()
		{

		}

		private void CalculateCurve()
		{
			for ( double t = 0.0 ; t <= 1.0 ; t += STEP )
			{
				var point = RecursiveCasteglio(t, m_points);
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

		private static Point Interpolate(Point begin, Point end, double t)
		{
			var x = (1.0 - t) * begin.X + t * end.X;
			var y = (1.0 - t) * begin.Y + t * end.Y;
			return new Point(x, y);
		}
	}
}
