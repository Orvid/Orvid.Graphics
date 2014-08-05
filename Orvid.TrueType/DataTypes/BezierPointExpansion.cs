using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	internal static class BezierPointExpansion
	{
		//public const double BezierStep = 0.05d;
		public const double BezierStep = 0.1d;

		#region Arbitrary
		public static Vec2d[] ExpandArbitraryBezier(Vec2d[] pts)
		{
			//return pts;
			switch (pts.Length)
			{
				case 0:
				case 1:
					throw new Exception("Too few points to draw!");
				case 2:
					return pts;
				case 3:
					return ExpandQuadraticBezier(pts[0], pts[1], pts[2]);
				case 4:
					return ExpandCubicBezier(pts[0], pts[1], pts[2], pts[3]);
				default:
					Vec2d[] points = new Vec2d[(int)(1.0 / BezierStep) + 2];
					points[0] = pts[0];
					int i2 = 1;
					Vec2d P;
					for (double t = 0.0; t <= 1.0; t += BezierStep, i2++)
					{
						double ti = 0;
						double tni = 0;
						double basis = 0;
						double pX = 0;
						double pY = 0;
						for (uint i = 0; i < pts.Length; i++)
						{
							if (t == 0 && i == 0)
								ti = 1;
							else
								ti = Math.Pow(t, i);
							if (pts.Length - 1 == i && t == 1)
								tni = 1;
							else
								tni = Math.Pow((1 - t), pts.Length - 1 - i);
							basis =
							  (
								MathUtils.Factorial((int)(pts.Length - 1))
								/
								(
								   MathUtils.Factorial((int)i)
								 * MathUtils.Factorial(((int)(pts.Length - 1)) - (int)i)
								)
							  )
							  * ti
							  * tni
							  ;
							pX += (basis * pts[i].X);
							pY += (basis * pts[i].Y);
						}
						P.X = pX;
						P.Y = pY;
						points[i2] = P;
					}
					return points;
			}
		}
		#endregion

		#region Quadratic
		public static Vec2d[] ExpandQuadraticBezier(Vec2d A, Vec2d B, Vec2d C)
		{
			Vec2d[] points = new Vec2d[(int)(1.0 / BezierStep) + 2];
			points[0] = A;
			int i = 1;
			Vec2d P;
			for (double t = 0.0; t <= 1.0; t += BezierStep, i++)
			{
				P.X = Math.Pow((1 - t), 2) * A.X + 2 * t * (1 - t) * B.X + Math.Pow(t, 2) * C.X;
				P.Y = Math.Pow((1 - t), 2) * A.Y + 2 * t * (1 - t) * B.Y + Math.Pow(t, 2) * C.Y;
				points[i] = P;
			}
			return points;
		}
		#endregion

		#region Cubic
		public static Vec2d[] ExpandCubicBezier(Vec2d A, Vec2d B, Vec2d C, Vec2d D)
		{
			Vec2d[] points = new Vec2d[(int)(1.0 / BezierStep) + 2];
			points[0] = A;
			int i = 1;
			Vec2d P;
			for (double t = 0.0; t <= 1.0; t += BezierStep, i++)
			{
				P.X = Math.Pow((1 - t), 3) * A.X + 3 * t * Math.Pow((1 - t), 2) * B.X + 3 * (1 - t) * Math.Pow(t, 2) * C.X + Math.Pow(t, 3) * D.X;
				P.Y = Math.Pow((1 - t), 3) * A.Y + 3 * t * Math.Pow((1 - t), 2) * B.Y + 3 * (1 - t) * Math.Pow(t, 2) * C.Y + Math.Pow(t, 3) * D.Y;
				points[i] = P;
			}
			return points;
		}
		#endregion

	}
}
