using System;

namespace Orvid.TrueType
{
    /// <summary>
    /// This class describes a single pixel.
    /// </summary>
    public struct Pixel
    {
        /// <summary>
        /// The byte that describes the amount of Red in the pixel.
        /// </summary>
        public byte R;
        /// <summary>
        /// The byte that describes the amount of Green in the pixel.
        /// </summary>
        public byte G;
        /// <summary>
        /// The byte that describes the amount of Blue in the pixel.
        /// </summary>
        public byte B;
        /// <summary>
        /// The byte that describes the transparency of the pixel.
        /// </summary>
        public byte A;
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

        public Pixel(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

		public override string ToString()
		{
			return "(" + R.ToString() + ", " + G.ToString() + ", " + B.ToString() + ", " + A.ToString() + ")";
		} 

		public static bool operator !=(Pixel a, Pixel b)
        {
            if (a.A != b.A || a.B != b.B || a.G != b.G || a.R != b.R)
                return true;
            return false;
        }

        public static bool operator ==(Pixel a, Pixel b)
        {
            if (a.A != b.A || a.B != b.B || a.G != b.G || a.R != b.R)
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
			if (obj is Pixel)
				return (this == (Pixel)obj);
			return false;
        }

        public override int GetHashCode()
        {
			return base.GetHashCode();
        }

    }
}
