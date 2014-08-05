using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Orvid.TrueType
{
	[StructLayout(LayoutKind.Explicit)]
	/// <summary>
	/// Represents a vector of <see cref="F26Dot6"/>
	/// units.
	/// </summary>
	public struct VecF26Dot6
	{
		public static readonly VecF26Dot6 Axis_X = new VecF26Dot6(1.0d, 0.0d);
		public static readonly VecF26Dot6 NegAxis_X = new VecF26Dot6(-1.0d, 0.0d);
		public static readonly VecF26Dot6 Axis_Y = new VecF26Dot6(0.0d, 1.0d);
		public static readonly VecF26Dot6 NegAxis_Y = new VecF26Dot6(0.0d, -1.0d);
		public static readonly VecF26Dot6 Zero = new VecF26Dot6(0.0d, 0.0d);

		// This is only public so that
		// the generated assemblies are
		// able to be fully verifiable
		// while still being fast.
		[FieldOffset(0)]
		public F26Dot6 local_x;
		/// <summary>
		/// The X position.
		/// </summary>
		public F26Dot6 X
		{
			get { return local_x; }
			set { local_x = value; }
		}

		// This is only public so that
		// the generated assemblies are
		// able to be fully verifiable
		// while still being fast.
		[FieldOffset(4)]
		public F26Dot6 local_y;
		/// <summary>
		/// The Y position.
		/// </summary>
		public F26Dot6 Y
		{
			get { return local_y; }
			set { local_y = value; }
		}

		public VecF26Dot6(F26Dot6 x, F26Dot6 y)
		{
			this.local_x = x;
			this.local_y = y;
		}

		/// <summary>
		///  Swaps the x & y coords.
		/// </summary>
		/// <returns>The new object.</returns>
		public VecF26Dot6 Swap()
		{
			return new VecF26Dot6(this.local_y, this.local_x);
		}

		/// <summary>
		/// Gets the point where the specified
		/// lines intersect.
		/// </summary>
		/// <param name="VecA_P1">The first point of the first line.</param>
		/// <param name="VecA_P2">The second point of the first line.</param>
		/// <param name="VecB_P1">The first point of the second line.</param>
		/// <param name="VecB_P2">The second point of the second line.</param>
		/// <returns>The point where the lines intersect.</returns>
		public static VecF26Dot6 GetIntersection(VecF26Dot6 VecA_P1, VecF26Dot6 VecA_P2, VecF26Dot6 VecB_P1, VecF26Dot6 VecB_P2)
		{
			F26Dot6 a, b, c;
			F26Dot6 d1, d2;
			F26Dot6 r;

			
			a = -(VecA_P2.Y - VecA_P1.Y);
			b = VecA_P2.X - VecA_P1.X;
			c = VecA_P1.X * a - VecA_P1.Y * b;

			d1 = a * VecB_P1.X + b * VecB_P1.Y + c;
			d2 = a * VecB_P2.X + b * VecB_P2.Y + c;

			r = d1 / (d2 - d1);
			return new VecF26Dot6(VecB_P1.X + (r * (VecB_P2.X - VecB_P1.X)), VecB_P1.Y + (r * (VecB_P2.Y - VecB_P1.Y)));
		}

		public override string ToString()
		{
			return "(" + local_x.ToString() + ", " + local_y.ToString() + ")";
		}

		public static explicit operator Vec2d(VecF26Dot6 vec)
		{
			return new Vec2d(F26Dot6.ToDouble(vec.X), F26Dot6.ToDouble(vec.Y));
		}

		public static explicit operator VecF26Dot6(Vec2d vec)
		{
			return new VecF26Dot6(F26Dot6.FromDouble(vec.X), F26Dot6.FromDouble(vec.Y));
		}

		public static VecF26Dot6 operator +(VecF26Dot6 a, VecF26Dot6 b)
		{
			return new VecF26Dot6(a.X + b.X, a.Y + b.Y);
		}

		public static VecF26Dot6 operator *(VecF26Dot6 a, F26Dot6 b)
		{
			return new VecF26Dot6(a.X * b, a.Y * b);
		}

		public static VecF26Dot6 operator *(F26Dot6 b, VecF26Dot6 a)
		{
			return new VecF26Dot6(a.X * b, a.Y * b);
		}

		public static VecF26Dot6 operator /(VecF26Dot6 a, int b)
		{
			return new VecF26Dot6(a.X / b, a.Y / b);
		}

		public static bool operator ==(VecF26Dot6 a, VecF26Dot6 b)
		{
			return (a.X == b.X && a.Y == b.Y);
		}

		public static bool operator !=(VecF26Dot6 a, VecF26Dot6 b)
		{
			return (a.X != b.X && a.Y != b.Y);
		}

		public override bool Equals(object obj)
		{
			if (obj is VecF26Dot6)
				return this == ((VecF26Dot6)obj);
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
