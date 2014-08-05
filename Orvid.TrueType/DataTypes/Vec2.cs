// I dislike empty xml comments, so I disable that warning.
#pragma warning disable 1591

using System;

namespace Orvid.TrueType
{
    /// <summary>
    /// This is a 2d vector. aka. A point on a 2d plane.
    /// </summary>
    public struct Vec2
    {
        public static Vec2 Zero = new Vec2(0, 0);

        /// <summary>
        /// The X position.
        /// </summary>
        public int X;
        /// <summary>
        /// The Y position.
        /// </summary>
        public int Y;

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="width">The X position.</param>
        /// <param name="height">The Y position.</param>
        public Vec2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vec2 operator -(Vec2 v)
		{
			return new Vec2(-v.X, -v.Y);
        }

        public static Vec2 operator -(Vec2 a, Vec2 b)
		{
			return new Vec2(a.X - b.X, a.Y - b.Y);
        }

        public static Vec2 operator -(Vec2 a, int b)
		{
			return new Vec2(a.X - b, a.Y - b);
        }

        public static Vec2 operator +(Vec2 a, Vec2 b)
		{
			return new Vec2(a.X + b.X, a.Y + b.Y);
		}

		public static Vec2 operator +(Vec2 a, int b)
		{
			return new Vec2(a.X + b, a.Y + b);
		}

        public static Vec2 operator /(Vec2 a, Vec2 b)
		{
			return new Vec2(a.X / b.X, a.Y / b.Y);
        }

        public static Vec2 operator /(Vec2 a, int b)
		{
			return new Vec2(a.X / b, a.Y / b);
        }

        public static bool operator !=(Vec2 a, Vec2 b)
        {
            if (a.X != b.X || a.Y != b.Y)
                return true;
            return false;
        }

        public static bool operator ==(Vec2 a, Vec2 b)
        {
            if (a.X != b.X || a.Y != b.Y)
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
			if (obj is Vec2)
				return (this == (Vec2)obj);
			return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public override string ToString()
		{
			return "(" + this.X.ToString() + ", " + this.Y.ToString() + ")";
		}
    }
}
// And restore the warning.
#pragma warning restore 1591