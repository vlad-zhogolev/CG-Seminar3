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
	public partial class SupportingPoint : MovableControl
	{
		public static readonly double RADIUS = 3;
		public static readonly double DIAMETER = 2 * RADIUS;

		public Point Coordinates { get; set; }

		public SupportingPoint(Point point)
		{
			InitializeComponent();
			Coordinates = point;
			LeftOffset = Coordinates.X - RADIUS;
			TopOffset = Coordinates.Y - RADIUS;
		}
	}
}
