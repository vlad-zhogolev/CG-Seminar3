using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spline
{
	class Utility
	{
		public static SupportingPoint CalculateSymmetricPoint(SupportingPoint symmetryCenter, SupportingPoint point)
		{
			double x = symmetryCenter.X - (point.X - symmetryCenter.X);
			double y = symmetryCenter.Y - (point.Y - symmetryCenter.Y);
			return new SupportingPoint(x, y);
		}
	}

	static class Extensions
	{
		public static IList<T> Clone<T>(this IList<T> list) where T : ICloneable
		{
			return list.Select(i => (T)i.Clone()).ToList();
		}
	}
}
