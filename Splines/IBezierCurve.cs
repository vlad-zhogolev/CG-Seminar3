using System.Windows.Controls;

namespace Spline
{
	interface IBezierCurve
	{
		void Draw(Canvas canvas);
		void Erase(Canvas canvas);
		void Open();
		void Close();
	}
}
