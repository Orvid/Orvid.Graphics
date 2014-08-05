using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	/// <summary>
	/// Represents a vector of <see cref="F2Dot14"/>
	/// units.
	/// </summary>
	public struct VecF2Dot14
	{
		public static readonly VecF2Dot14 Axis_X = new VecF2Dot14(1.0d, 0.0d);
		public static readonly VecF2Dot14 NegAxis_X = new VecF2Dot14(-1.0d, 0.0d);
		public static readonly VecF2Dot14 Axis_Y = new VecF2Dot14(0.0d, 1.0d);
		public static readonly VecF2Dot14 NegAxis_Y = new VecF2Dot14(0.0d, -1.0d);
		public static readonly VecF2Dot14 Zero = new VecF2Dot14(0.0d, 0.0d);

		// This is only public so that
		// the generated assemblies are
		// able to be fully verifiable
		// while still being fast.
		public F2Dot14 local_x;
		/// <summary>
		/// The X position.
		/// </summary>
		public F2Dot14 X
		{
			get { return local_x; }
			set { local_x = value; }
		}

		// This is only public so that
		// the generated assemblies are
		// able to be fully verifiable
		// while still being fast.
		public F2Dot14 local_y;
		/// <summary>
		/// The Y position.
		/// </summary>
		public F2Dot14 Y
		{
			get { return local_y; }
			set { local_y = value; }
		}

		public VecF2Dot14(F2Dot14 x, F2Dot14 y)
		{
			this.local_x = x;
			this.local_y = y;
		}

		/// <summary>
		///  Swaps the x & y coords.
		/// </summary>
		/// <returns>The new object.</returns>
		public VecF2Dot14 Swap()
		{
			return new VecF2Dot14(this.local_y, this.local_x);
		}
		
		public static bool operator ==(VecF2Dot14 a, VecF2Dot14 b)
		{
			return (a.X == b.X && a.Y == b.Y);
		}

		public static bool operator !=(VecF2Dot14 a, VecF2Dot14 b)
		{
			return (a.X != b.X && a.Y != b.Y);
		}

		public override bool Equals(object obj)
		{
			if (obj is VecF2Dot14)
				return this == ((VecF2Dot14)obj);
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return "(" + local_x.ToString() + ", " + local_y.ToString() + ")";
		}
	}
}
