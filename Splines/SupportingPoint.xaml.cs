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
	/// Interaction logic for SupportingPoint.xaml
	/// </summary>
	public partial class SupportingPoint : UserControl
	{
		private Point m_coordinates;

		public Point Coordinates
		{
			get
			{
				return m_coordinates;			
			}
			set
			{
				m_coordinates = value;
				//Canvas.SetLeft(this, m_coordinates.X);
				//Canvas.SetTop(this, m_coordinates.Y);
			}
		}

		public SupportingPoint(Point point)
		{
			Width = 5;
			Height = 5;
			Coordinates = point;
			Ellipse = new Ellipse();
			Ellipse.Width = Constants.POINT_DIAMETER;
			Ellipse.Height = Constants.POINT_DIAMETER;
			Ellipse.Fill = Constants.POINT_COLOR;

			Canvas.SetLeft(this, m_coordinates.X);
			Canvas.SetTop(this, m_coordinates.Y);
		}

		public SupportingPoint(int x, int y) : this(new Point(x, y)) {}
		
		public SupportingPoint()
		{
			InitializeComponent();
		}
	}
}
