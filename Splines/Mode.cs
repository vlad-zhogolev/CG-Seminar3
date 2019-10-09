using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spline
{
	enum CursorMode
	{
		SettingPoints,
		MovingPoints,
	}

	enum DrawingMode
	{
		Default,
		Castiglio,
		DefaultWithCastiglio,
	}

	enum BezierLineType
	{
		ArbitraryOrder,
		ThirdOrder,
		CompositeThirdOrder,
	}
}
