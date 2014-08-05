// I dislike empty xml comments, so I disable that warning.
#pragma warning disable 1591

using System;
using System.Runtime.InteropServices;

namespace Orvid.TrueType
{
	[StructLayout(LayoutKind.Explicit)]
	/// <summary>
    /// This is a 2d vector. aka. A point on a 2d plane.
    /// </summary>
    public struct Vec2d
    {
        public static Vec2d Zero = new Vec2d(0, 0);

		[FieldOffset(0)]
        /// <summary>
        /// The X position.
        /// </summary>
        public double X;
		[FieldOffset(8)]
        /// <summary>
        /// The Y position.
        /// </summary>
        public double Y;

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        public Vec2d(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vec2d operator -(Vec2d v)
		{
			return new Vec2d(-v.X, -v.Y);
        }

        public static Vec2d operator -(Vec2d a, Vec2d b)
        {
			return new Vec2d(a.X - b.X, a.Y - b.Y);
        }

        public static Vec2d operator -(Vec2d a, double b)
		{
			return new Vec2d(a.X - b, a.Y - b);
        }

        public static Vec2d operator +(Vec2d a, Vec2d b)
		{
			return new Vec2d(a.X + b.X, a.Y + b.Y);
		}

		public static Vec2d operator +(Vec2d a, double b)
		{
			return new Vec2d(a.X + b, a.Y + b);
		}

        public static Vec2d operator /(Vec2d a, Vec2d b)
		{
			return new Vec2d(a.X / b.X, a.Y / b.Y);
        }

        public static Vec2d operator /(Vec2d v, double s)
		{
			return new Vec2d(v.X / s, v.Y / s);
        }

        public static bool operator !=(Vec2d a, Vec2d b)
        {
            if (a.X != b.X || a.Y != b.Y)
                return true;
            return false;
        }

        public static bool operator ==(Vec2d a, Vec2d b)
        {
            if (a.X != b.X || a.Y != b.Y)
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
			if (obj is Vec2d)
				return (this == (Vec2d)obj);
			return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
		}

		public static explicit operator Vec2(Vec2d a)
		{
			Vec2 v = new Vec2();
			v.X = (int)Math.Round(a.X);
			v.Y = (int)Math.Round(a.Y);
			return v;
		}

		public override string ToString()
		{
			return X.ToString() + ", " + Y.ToString();
		}
    }
}
// And restore the warning.
#pragma warning restore 1591
