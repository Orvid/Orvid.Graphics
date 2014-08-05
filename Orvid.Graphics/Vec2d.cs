// I dislike empty xml comments, so I disable that warning.
#pragma warning disable 1591

using System;
using Orvid.TrueType;

namespace Orvid.Graphics
{
	/// <summary>
    /// This is a 2d vector. aka. A point on a 2d plane.
    /// </summary>
    public struct Vec2d
    {
        public static Vec2d Zero = new Vec2d(0, 0);

        /// <summary>
        /// The X position.
        /// </summary>
        public double X;
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
            Vec2d vec = new Vec2d();
            vec.X = -v.X;
            vec.Y = -v.Y;
            return vec;
        }

        public static Vec2d operator -(Vec2d a, Vec2d b)
        {
            Vec2d v = new Vec2d();
            v.X = a.X - b.X;
            v.Y = a.Y - b.Y;
            return v;
        }

        public static Vec2d operator -(Vec2d a, double b)
        {
            Vec2d v = new Vec2d();
            v.X = a.X - b;
            v.Y = a.Y - b;
            return v;
        }

        public static Vec2d operator +(Vec2d a, Vec2d b)
        {
            Vec2d v = new Vec2d();
            v.X = a.X + b.X;
            v.Y = a.Y + b.Y;
            return v;
        }

        public static Vec2d operator /(Vec2d a, Vec2d b)
        {
            Vec2d v = new Vec2d();
            v.X = a.X / b.X;
            v.Y = a.Y / b.Y;
            return v;
        }

        public static Vec2d operator /(Vec2d v, double s)
        {
            Vec2d vec = new Vec2d();
            // Division of doubles can be expensive,
            // so we multiply instead, as it's much cheaper.
            double num = 1f / s;
            vec.X = v.X * num;
            vec.Y = v.Y * num;
            return vec;
        }

        public static Vec2d operator /(double s, Vec2d v)
        {
            Vec2d vec = new Vec2d();
            vec.X = s / v.X;
            vec.Y = s / v.Y;
            return vec;
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
        
        public static implicit operator Vec2(Vec2d a)
        {
        	Vec2 v = new Vec2();
        	v.X = (int)Math.Round(a.X);
			v.Y = (int)Math.Round(a.Y);
        	return v;
        }

        public override bool Equals(object obj)
        {
            return (this == (Vec2d)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public static explicit operator VecF26Dot6(Vec2d val)
		{
			VecF26Dot6 o = new VecF26Dot6();
			o.X = F26Dot6.FromDouble(val.X);
			o.Y = F26Dot6.FromDouble(val.Y);
			return o;
		}
    }
}
// And restore the warning.
#pragma warning restore 1591
