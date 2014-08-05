using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics
{
	/// <summary>
	/// Represents a triangle.
	/// </summary>
	public struct Triangle
	{
		public Vec2 PointA;
		public Vec2 PointB;
		public Vec2 PointC;

		public Triangle(Vec2 A, Vec2 B, Vec2 C)
		{
			this.PointA = A;
			this.PointB = B;
			this.PointC = C;
		}

		private static bool SameSide(Vec2 p1, Vec2 p2, Vec2 LpA, Vec2 LpB)
		{
			return (
				((p1.X - LpA.X) * (LpB.Y - LpA.Y) - (LpB.X - LpA.X) * (p1.Y - LpA.Y))
				*
				((p2.X - LpA.X) * (LpB.Y - LpA.Y) - (LpB.X - LpA.X) * (p2.Y - LpA.Y))
				) > 0;
		}

		/// <summary>
		/// Returns true if the specified point is
		/// in this Triangle.
		/// </summary>
		/// <param name="p">The point to check.</param>
		/// <returns>True if the point is in this triangle.</returns>
		public bool IsInTriangle(Vec2 p)
		{
			if (p.Y < Utils.GetMin(PointA.Y, PointB.Y, PointC.Y))
				return false;
			if (p.Y > Utils.GetMax(PointA.Y, PointB.Y, PointC.Y))
				return false;
			if (p.X < Utils.GetMin(PointA.X, PointB.X, PointC.X))
				return false;
			if (p.X > Utils.GetMax(PointA.X, PointB.X, PointC.X))
				return false;

			return (
				SameSide(p, PointA, PointB, PointC) &&
				SameSide(p, PointB, PointA, PointC) && 
				SameSide(p, PointC, PointA, PointB)
				);
		}

		public override string ToString()
		{
			return PointA.ToString() + " " + PointB.ToString() + " " + PointC.ToString();
		}
	}
}
