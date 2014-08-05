using System;

namespace Orvid.TrueType
{
    public static class Colors
    {
		public static readonly Pixel Black = new Pixel(0x00, 0x00, 0x00, 255);
		public static readonly Pixel Blue = new Pixel(0x00, 0x00, 0xFF, 255);
		public static readonly Pixel Green = new Pixel(0x00, 0xFF, 0x00, 255);
		public static readonly Pixel Red = new Pixel(0xFF, 0x00, 0x00, 255);
		public static readonly Pixel Transparent = new Pixel(0x00, 0x00, 0x00, 0x00);
        public static readonly Pixel White = new Pixel(0xFF, 0xFF, 0xFF, 255);
    }
}
