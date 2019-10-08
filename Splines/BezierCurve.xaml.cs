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
	/// <summary>
	/// Interaction logic for BezierCurve.xaml
	/// </summary>
	public partial class BezierCurve : UserControl
	{
		public static readonly double STEP = 0.01;
		private IList<Point> m_supportingPoints;
		private double[,] m_supportingPointsMatrix;
		private double[,] m_baseMatrix;

		public BezierCurve(IList<Point> points)
		{
			InitializeComponent();
			m_supportingPoints = points;
			CalculateCurve();
		}

		public void AddPoint(Point point)
		{
			m_supportingPoints.Add(point);
		}

		public void CalculateCurve()
		{
			if (m_supportingPoints.Count < 1)
			{
				throw new ArgumentOutOfRangeException("Number of supporting points must be greater than 1");
			}

			CreateMatrixFromPoints();
			CalculateBaseMatrix();
			var PM = MultiplyMatrices(m_supportingPointsMatrix, m_baseMatrix);

			for (double t = 0.0 ; !AboutEqual(t, 1.0 + STEP) ; t += STEP )
			{
				var stepMatrix = CreateMatrixFromStep(t);
				var pointVector = MultiplyMatrices(PM, stepMatrix); // it will have 2 * 1 size
				var point = new System.Windows.Point() { X = pointVector[0, 0], Y = pointVector[1, 0] };
				Curve.Points.Add(point);

				//var point = new System.Windows.Point()
				//{
				//	X = Math.Pow(1 - t, 3) * m_supportingPoints[0].X + 3 * t * Math.Pow(1 - t, 2) * m_supportingPoints[1].X + 3 * t * t * (1 - t) * m_supportingPoints[2].X + t * t * t * m_supportingPoints[3].X,
				//	Y = Math.Pow(1 - t, 3) * m_supportingPoints[0].Y + 3 * t * Math.Pow(1 - t, 2) * m_supportingPoints[1].Y + 3 * t * t * (1 - t) * m_supportingPoints[2].Y + t * t * t * m_supportingPoints[3].Y,
				//};
				//Curve.Points.Add(point);
			}
			//Curve.Points.Add(m_supportingPoints[m_supportingPoints.Count - 1]);
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

		public static bool AboutEqual(double x, double y)
		{
			double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
			return Math.Abs(x - y) <= epsilon;
		}
	}
}
