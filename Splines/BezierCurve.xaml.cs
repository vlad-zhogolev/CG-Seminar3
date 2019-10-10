using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.ComponentModel;

namespace Spline
{
	/// <summary>
	/// Interaction logic for BezierCurve.xaml
	/// </summary>
	public partial class BezierCurve : UserControl, IBezierCurve
	{
		public static readonly double STEP = 0.005;
		private IList<SupportingPoint> m_supportingPoints;
		private double[,] m_supportingPointsMatrix;
		private double[,] m_baseMatrix;
		private bool m_isClosed;
		private Canvas m_canvas;

		public BezierCurve(IList<SupportingPoint> points, Canvas canvas, bool isClosed = false)
		{
			if (points.Count < 2)
			{
				throw new ArgumentException("Bezier curve must have at least 2 supporting points");
			}
			InitializeComponent();
			m_supportingPoints = points;
			m_isClosed = isClosed;
			m_canvas = canvas;
			foreach(var point in m_supportingPoints)
			{
				point.PropertyChanged += PointPositionChanged;
			}
			if (m_isClosed)
			{
				CloseCurve();
			}
			CalculateCurve();
		}

		public void Draw(Canvas canvas)
		{
			canvas.Children.Add(this);
			var size = m_isClosed ? m_supportingPoints.Count - 1 : m_supportingPoints.Count;
			for (var i = 0 ; i < size ; ++i)
			{
				canvas.Children.Add(m_supportingPoints[i]);
			}	
		}

		public void Erase(Canvas canvas)
		{
			var size = m_isClosed ? m_supportingPoints.Count - 1 : m_supportingPoints.Count;
			for ( var i = 0 ; i < size ; ++i )
			{
				canvas.Children.Remove(m_supportingPoints[i]);
			}
			canvas.Children.Remove(this);
		}

		public bool IsClosed
		{
			get {return m_isClosed;}
			set {m_isClosed = value;}
		}

		public void Close()
		{
			if (!m_isClosed)
			{
				m_isClosed = true;
				CloseCurve();
				CalculateCurve();
			}
		}

		public void Open()
		{
			if (m_isClosed)
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
			if (m_supportingPoints.Count < 1)
			{
				throw new ArgumentOutOfRangeException("Number of supporting points must be greater than 1");
			}
			Curve.Points.Clear();
			Border.Points.Clear();
			CreateMatrixFromPoints();
			CalculateBaseMatrix();
			var PM = MultiplyMatrices(m_supportingPointsMatrix, m_baseMatrix);
			for (double t = 0.0 ; !AboutEqual(t, 1.0 + STEP) ; t += STEP )
			{
				var stepMatrix = CreateMatrixFromStep(t);
				var pointVector = MultiplyMatrices(PM, stepMatrix); // it will have 2 * 1 size
				var point = new System.Windows.Point() { X = pointVector[0, 0], Y = pointVector[1, 0] };
				Curve.Points.Add(point);
			}

			for ( var i = 0 ; i < m_supportingPoints.Count ; ++i )
			{
				var borderPoint = new System.Windows.Point() { X = m_supportingPoints[i].X, Y = m_supportingPoints[i].Y };
				Border.Points.Add(borderPoint);
			}
			var lastBorderPoint = new System.Windows.Point() { X = m_supportingPoints[0].X, Y = m_supportingPoints[0].Y };
			Border.Points.Add(lastBorderPoint);
		}

		private void CalculateBaseMatrix()
		{
			int n = m_supportingPoints.Count;
			m_baseMatrix = new double[n, n];

			m_baseMatrix[0, 0] = 1;
			for ( int j = 1 ; j < n; ++j )
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
			if (a.GetLength(1) != b.GetLength(0))
			{
				throw new ArgumentException("Matrices have incompatible size");			
			}
			var result = new double[a.GetLength(0), b.GetLength(1)];
			for (int i = 0 ; i < a.GetLength(0) ; ++i )
			{
				for (int j = 0 ; j < b.GetLength(1) ; ++j )
				{					
					for (int k = 0 ; k < a.GetLength(1) ; ++k )
					{
						result[i, j] += a[i, k] * b[k, j];
					}
				}
			}
			return result;
		}

		private void CreateMatrixFromPoints()
		{
			var result = new double[2, m_supportingPoints.Count];
			for (int i = 0 ; i < m_supportingPoints.Count ; ++i)
			{
				result[0, i] = m_supportingPoints[i].X;
				result[1, i] = m_supportingPoints[i].Y;
			}			
			m_supportingPointsMatrix = result;
		}

		private double[,] CreateMatrixFromStep(double value)
		{
			var result = new double[m_supportingPoints.Count, 1];
			for (int i = 0 ; i < result.GetLength(0) ; ++i)
			{
				result[i, 0] = Math.Pow(value, i);
			}
			return result;
		}

		private void CloseCurve()
		{
			var point = CalculateSymmetricPoint(m_supportingPoints[0], m_supportingPoints[1]);
			point.PropertyChanged += PointPositionChanged;
			m_supportingPoints.Add(point);
			m_supportingPoints.Add(m_supportingPoints[0]);
		}

		private void OpenCurve()
		{
			m_supportingPoints.RemoveAt(m_supportingPoints.Count - 1);
			m_supportingPoints.RemoveAt(m_supportingPoints.Count - 1);
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
