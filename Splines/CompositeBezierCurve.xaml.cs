using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.ComponentModel;

namespace Spline
{
	public class Edge
	{
		private IList<SupportingPoint> m_points = new List<SupportingPoint>();

		public IList<SupportingPoint> Points
		{
			get
			{
				return m_points;
			}
			set
			{
				if ( value.Count != 3 )
				{
					throw new ArgumentException("Edge must contain 3 points");
				}
				m_points = value;
			}
		}

		public SupportingPoint Begin
		{
			get
			{
				return Points[0];
			}
			set
			{
				Points[0] = value;
			}
		}

		public SupportingPoint Middle
		{
			get
			{
				return Points[1];
			}
			set
			{
				Points[1] = value;
			}
		}

		public SupportingPoint End
		{
			get
			{
				return Points[2];
			}
			set
			{
				Points[2] = value;
			}
		}

		public Edge() { }

		public Edge(SupportingPoint begin, SupportingPoint middle, SupportingPoint end)
		{
			Points.Add(begin);
			Points.Add(middle);
			Points.Add(end);
		}

		public Edge(IList<SupportingPoint> points)
		{
			if (points.Count != 3)
			{
				throw new ArgumentException("Edge must contain 3 points");
			}
			m_points = points;
		}		
	}
	/// <summary>
	/// Interaction logic for CompositeBezierCurve.xaml
	/// </summary>
	public partial class CompositeBezierCurve : UserControl, IBezierCurve
	{
		public static readonly double STEP = 0.05;
		private IList<SupportingPoint> m_supportingPoints;
		private IList<Edge> m_edges;	
		private double[,] m_baseMatrix;
		private bool m_isClosed;
		private Canvas m_canvas;

		public CompositeBezierCurve(IList<SupportingPoint> points, Canvas canvas, bool isClosed = false)
		{
			if ( points.Count < 6 )
			{
				throw new ArgumentException("Composite bezier curve must have at least  supporting points");
			}
			InitializeComponent();
			CalculateBaseMatrix(); // one matrix for everything
			m_supportingPoints = points;
			m_isClosed = isClosed;
			m_canvas = canvas;
			m_edges = new List<Edge>();	
			for(int i = 0 ; i < m_supportingPoints.Count ; ++i)
			{
				if (i % 3 == 0)
				{
					m_edges.Add(new Edge());
				}
				m_supportingPoints[i].PropertyChanged += PointPositionChanged;
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
			for ( var i = 0 ; i < m_supportingPoints.Count ; ++i )
			{
				canvas.Children.Add(m_supportingPoints[i]);
			}
		}

		public void Erase(Canvas canvas)
		{			
			for ( var i = 0 ; i < m_supportingPoints.Count ; ++i )
			{
				canvas.Children.Remove(m_supportingPoints[i]);
			}
			canvas.Children.Remove(this);
		}

		public bool IsClosed
		{
			get { return m_isClosed; }
			set { m_isClosed = value; }
		}

		public void Close()
		{
			if ( !m_isClosed )
			{
				m_isClosed = true;
				CloseCurve();
				CalculateCurve();
			}
		}

		public void Open()
		{
			if ( m_isClosed )
			{
				m_isClosed = false;
				OpenCurve();
				CalculateCurve();
			}
		}

		public void AddPoint(SupportingPoint point)
		{
			m_supportingPoints.Add(point);
		}

		public void CalculateCurve()
		{
			if ( m_supportingPoints.Count < 1 )
			{
				throw new ArgumentOutOfRangeException("Number of supporting points must be greater than 1");
			}
			Curve.Points.Clear();

			for ( int i = 0 ; i < m_edges.Count - 1 ; ++i )
			{
				var segmentPoints = CreateMatrixFromPoints(m_edges[i], m_edges[i + 1]);
				var PM = MultiplyMatrices(segmentPoints, m_baseMatrix);
				for ( double t = 0.0 ; !AboutEqual(t, 1.0 + STEP) ; t += STEP )
				{
					var stepMatrix = CreateMatrixFromStep(t);
					var pointVector = MultiplyMatrices(PM, stepMatrix); // it will have 2 * 1 size
					var point = new System.Windows.Point() { X = pointVector[0, 0], Y = pointVector[1, 0] };
					Curve.Points.Add(point);
				}
			}			
		}

		private void CalculateBaseMatrix()
		{
			int n = 4;
			m_baseMatrix = new double[n, n];

			m_baseMatrix[0, 0] = 1;
			for ( int j = 1 ; j < n ; ++j )
			{
				m_baseMatrix[0, j] = NextElementInRow(m_baseMatrix[0, j - 1], n - 1, 0, j - 1);
			}
			for ( int i = 1 ; i < n ; ++i )
			{
				m_baseMatrix[i, i] = NextDiagonalElement(m_baseMatrix[i - 1, i - 1], n - 1, i - 1);
				for ( int j = i + 1 ; j < n ; ++j )
				{
					m_baseMatrix[i, j] = NextElementInRow(m_baseMatrix[i, j - 1], n - 1, i, j - 1);
				}
			}
		}

		private double NextElementInRow(double value, int n, int i, int j)
		{
			return value * (-1) * (n - j) / (j - i + 1);
		}

		private double NextDiagonalElement(double value, int n, int i)
		{
			return value * (n - i) / (i + 1);
		}

		private static double[,] MultiplyMatrices(double[,] a, double[,] b)
		{
			if ( a.GetLength(1) != b.GetLength(0) )
			{
				throw new ArgumentException("Matrices have incompatible size");
			}
			var result = new double[a.GetLength(0), b.GetLength(1)];
			for ( int i = 0 ; i < a.GetLength(0) ; ++i )
			{
				for ( int j = 0 ; j < b.GetLength(1) ; ++j )
				{
					for ( int k = 0 ; k < a.GetLength(1) ; ++k )
					{
						result[i, j] += a[i, k] * b[k, j];
					}
				}
			}
			return result;
		}

		private double [,] CreateMatrixFromPoints(Edge first, Edge second)
		{
			var result = new double[2, 4];
			result[0, 0] = first.Middle.X;
			result[1, 0] = first.Middle.Y;
			result[0, 1] = first.End.X;
			result[1, 1] = first.End.Y;
			result[0, 2] = second.Begin.X;
			result[1, 2] = second.Begin.Y;
			result[0, 3] = second.Middle.X;
			result[1, 3] = second.Middle.Y;
			return result;
		}

		private double[,] CreateMatrixFromStep(double value)
		{
			var result = new double[4, 1];
			for ( int i = 0 ; i < result.GetLength(0) ; ++i )
			{
				result[i, 0] = Math.Pow(value, i);
			}
			return result;
		}

		private void CloseCurve()
		{
			var edge = new Edge(m_supportingPoints[0],
							m_supportingPoints[1],
							m_supportingPoints[2]);
			m_edges.Add(edge);
		}

		private void OpenCurve()
		{			
			m_edges.RemoveAt(m_edges.Count - 1);			
		}


		private void PointPositionChanged(object sender, PropertyChangedEventArgs e)
		{
			Erase(m_canvas);
			CalculateCurve();
			Draw(m_canvas);
		}

		public static bool AboutEqual(double x, double y)
		{
			double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
			return Math.Abs(x - y) <= epsilon;
		}

		private static SupportingPoint CalculateSymmetricPoint(SupportingPoint symmetryCenter, SupportingPoint point)
		{
			double x = symmetryCenter.X - (point.X - symmetryCenter.X);
			double y = symmetryCenter.Y - (point.Y - symmetryCenter.Y);
			return new SupportingPoint(x, y);
		}
	}
}
