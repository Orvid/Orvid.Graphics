using System;

namespace Orvid.Graphics
{
    /// <summary>
    /// This class describes a single pixel.
    /// </summary>
    public struct PixelF
    {
        /// <summary>
        /// The float that describes the amount of Red in the pixel.
        /// </summary>
        public float R;
        /// <summary>
        /// The byte that describes the amount of Green in the pixel.
        /// </summary>
        public float G;
        /// <summary>
        /// The byte that describes the amount of Blue in the pixel.
        /// </summary>
        public float B;
        /// <summary>
        /// The byte that describes the transparency of the pixel.
        /// </summary>
        public float A;
        /// <summary>
        /// This tells if the pixel is empty, and should be ignored.
        /// </summary>
        public bool Empty
        {
            get
            {
                return (A + R + B + G == 0);
            }
        }

        //public Pixel()
        //    : this(true)
        //{
        //}

        public PixelF(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public PixelF(bool empty)
        {
            this.R = 0;
            this.G = 0;
            this.B = 0;
            this.A = 0;
		}

		public override string ToString()
		{
			return "(" + R.ToString() + ", " + G.ToString() + ", " + B.ToString() + ", " + A.ToString() + ")";
		} 

		public static implicit operator PixelF(Pixel a)
		{
			return new PixelF(a.R, a.G, a.B, a.A);
		}

		public static implicit operator Pixel(PixelF a)
		{
			return new Pixel((byte)a.R, (byte)a.G, (byte)a.B, (byte)a.A);
		}

		#region Operators
		public static PixelF operator +(PixelF a, PixelF b)
		{
			PixelF c = a;
			c.A += b.A;
			c.B += b.B;
			c.G += b.G;
			c.R += b.R;
			return c;
		}

		public static PixelF operator +(PixelF a, byte b)
		{
			PixelF c = a;
			c.A += b;
			c.B += b;
			c.G += b;
			c.R += b;
			return c;
		}

		public static PixelF operator +(PixelF a, int b)
		{
			PixelF c = a;
			c.A += b;
			c.B += b;
			c.G += b;
			c.R += b;
			return c;
		}

		public static PixelF operator -(PixelF a, PixelF b)
		{
			PixelF c = a;
			c.A -= b.A;
			c.B -= b.B;
			c.G -= b.G;
			c.R -= b.R;
			return c;
		}

		public static PixelF operator -(PixelF a, byte b)
		{
			PixelF c = a;
			c.A -= b;
			c.B -= b;
			c.G -= b;
			c.R -= b;
			return c;
		}

		public static PixelF operator -(PixelF a, int b)
		{
			PixelF c = a;
			c.A -= b;
			c.B -= b;
			c.G -= b;
			c.R -= b;
			return c;
		}

		public static PixelF operator /(PixelF a, PixelF b)
		{
			PixelF c = a;
			c.A /= b.A;
			c.B /= b.B;
			c.G /= b.G;
			c.R /= b.R;
			return c;
		}

		public static PixelF operator /(PixelF a, Pixel b)
		{
			PixelF c = a;
			c.A /= b.A;
			c.B /= b.B;
			c.G /= b.G;
			c.R /= b.R;
			return c;
		}

		public static PixelF operator /(PixelF a, byte b)
		{
			PixelF c = a;
			c.A /= b;
			c.B /= b;
			c.G /= b;
			c.R /= b;
			return c;
		}

		public static PixelF operator /(PixelF a, int b)
		{
			PixelF c = a;
			c.A /= b;
			c.B /= b;
			c.G /= b;
			c.R /= b;
			return c;
		}

		public static PixelF operator *(PixelF a, PixelF b)
		{

			PixelF c = a;
			c.A *= b.A;
			c.B *= b.B;
			c.G *= b.G;
			c.R *= b.R;
			return c;
		}

		public static PixelF operator *(PixelF a, byte b)
		{
			PixelF c = a;
			c.A *= b;
			c.B *= b;
			c.G *= b;
			c.R *= b;
			return c;
		}

		public static PixelF operator *(PixelF a, int b)
		{
			PixelF c = a;
			c.A *= b;
			c.B *= b;
			c.G *= b;
			c.R *= b;
			return c;
		}
		#endregion

		public static bool operator !=(PixelF a, PixelF b)
        {
            //if (!(a is Pixel) || !(b is Pixel))
            //{
            //    if (!(a is Pixel) && !(b is Pixel))
            //        return false;
            //    return true;
            //}
            //else
            //{
            if (a.A != b.A || a.B != b.B || a.G != b.G || a.R != b.R)
                return true;
            return false;
            //}
        }

        public static bool operator !=(PixelF a, int b)
        {
            return (a.A != b && a.B != b && a.G != b && a.R != b);
        }

        public static bool operator ==(PixelF a, int b)
        {
            return (a.A == b && a.B == b && a.G == b && a.R == b);
        }

        public static bool operator ==(PixelF a, PixelF b)
        {

            //if (!(a is Pixel) || !(b is Pixel))
            //{
            //    if (!(a is Pixel) && !(b is Pixel))
            //        return true;
            //    return false;
            //}
            //else
            //{
            if (a.A != b.A || a.B != b.B || a.G != b.G || a.R != b.R)
                return false;
            return true;
            //}
        }

        public override bool Equals(object obj)
        {
            return (this == (PixelF)obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
