using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	public class PointZone
	{
		public bool[] TouchedX;
		public bool[] TouchedY;
		public int[] EndPointsOfContours;
		public int ContourCount;
		public int PointCount;
		public VecF26Dot6[] Contour;
		public VecF26Dot6[] OriginalContour;
		public VecF26Dot6[] OriginalUnscaledContour;
		public bool[] OnCurve;

	}
}
