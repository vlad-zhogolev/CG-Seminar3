using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Spline
{
	public class MovableControl : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty LeftOffsetProperty;

		public static readonly DependencyProperty TopOffsetProperty;

		static MovableControl()
		{
			LeftOffsetProperty =
			DependencyProperty.Register("LeftOffset",
				typeof(double), typeof(MovableControl),
				new PropertyMetadata(double.NaN, new PropertyChangedCallback(OffsetPropertyChangedCallback)));

			TopOffsetProperty =
			DependencyProperty.Register("TopOffset",
				typeof(double), typeof(MovableControl),
				new PropertyMetadata(double.NaN, new PropertyChangedCallback(OffsetPropertyChangedCallback)));
		}

		public MovableControl() {}

		public double LeftOffset
		{
			get { return (double)GetValue(LeftOffsetProperty); }
			set { SetValue(LeftOffsetProperty, value); }
		}

		public double TopOffset
		{
			get { return (double)GetValue(TopOffsetProperty); }
			set { SetValue(TopOffsetProperty, value); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private static void OffsetPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = d as MovableControl;
			control.OnPropertyChanged(e.Property.Name);
			control.OnPropertyChanged("CenterPoint");
		}
		protected void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}
