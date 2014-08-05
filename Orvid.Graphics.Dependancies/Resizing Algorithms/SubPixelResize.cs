using System;
using System.Collections.Generic;

namespace Orvid.TrueType
{
	public static class SubPixelResize
	{

		#region Resize
		//private const double aNinth = (double)(255d / 9d);
		//private const double bNinth = (double)((255d / 9d) * 2);
		//private const double cNinth = (double)((255d / 9d) * 3);
		private const double aFifth = (double)(255d / 5d);
		private const double bFifth = (double)((255d / 5d) * 3);

		public static Image Resize(Image img)
		{
			Image o = new Image((int)Math.Ceiling(img.Width / 3d), img.Height);
			o.Clear(Colors.Black);
			uint curIndBase = 0;
			uint x2 = 0;

			for (uint y = 0; y < img.Height; y++)
			{
				x2 = 0;
				for (uint x = 0; x < img.Width; x += 3)
				{
					if (img.GetPixel(x, y) == Colors.White)
					{
						if (x2 > 1)
						{
							o.Data[curIndBase - 1].R = AddWithoutOverflow((byte)aFifth, o.Data[curIndBase - 1].R);
						}
						//else
						//{
						//    o.Data[curIndBase].R = AddWithoutOverflow((byte)aFifth * 2, o.Data[curIndBase].R);
						//}
						o.Data[curIndBase].R = AddWithoutOverflow((byte)bFifth, o.Data[curIndBase].R);
						//o.Data[curIndBase].R = 255;
						if (x2 < o.Width - 1)
						{
							o.Data[curIndBase + 1].R = AddWithoutOverflow((byte)aFifth, o.Data[curIndBase + 1].R);
						}
						//else
						//{
						//    o.Data[curIndBase].R = AddWithoutOverflow((byte)aFifth * 2, o.Data[curIndBase].R);
						//}
					}
					if (img.GetPixel(x + 1, y) == Colors.White)
					{
						if (x2 > 1)
						{
							o.Data[curIndBase - 1].G = AddWithoutOverflow((byte)aFifth, o.Data[curIndBase - 1].G);
						}
						//else
						//{
						//    o.Data[curIndBase].G = AddWithoutOverflow((byte)aFifth * 2, o.Data[curIndBase].G);
						//}
						o.Data[curIndBase].G = AddWithoutOverflow((byte)bFifth, o.Data[curIndBase].G);
						//o.Data[curIndBase].G = 255;
						if (x2 < o.Width - 1)
						{
							o.Data[curIndBase + 1].G = AddWithoutOverflow((byte)aFifth, o.Data[curIndBase + 1].G);
						}
						//else
						//{
						//    o.Data[curIndBase].G = AddWithoutOverflow((byte)aFifth * 2, o.Data[curIndBase].G);
						//}
					}
					if (img.GetPixel(x + 2, y) == Colors.White)
					{
						if (x2 > 1)
						{
							o.Data[curIndBase - 1].B = AddWithoutOverflow((byte)aFifth, o.Data[curIndBase - 1].B);
						}
						//else
						//{
						//    o.Data[curIndBase].B = AddWithoutOverflow((byte)aFifth * 2, o.Data[curIndBase].B);
						//}
						o.Data[curIndBase].B = AddWithoutOverflow((byte)bFifth, o.Data[curIndBase].B);
						//o.Data[curIndBase].B = 255;
						if (x2 < o.Width - 1)
						{
							o.Data[curIndBase + 1].B = AddWithoutOverflow((byte)aFifth, o.Data[curIndBase + 1].B);
						}
						//else
						//{
						//    o.Data[curIndBase].B = AddWithoutOverflow((byte)aFifth * 2, o.Data[curIndBase].B);
						//}
					}


					curIndBase++;
					x2++;
				}
			}

			return o;
		}

		private static byte AddWithoutOverflow(byte a, byte b)
		{
			if (a + b > 255)
			{
				return 255;
			}
			else
			{
				return (byte)(a + b);
			}
		}
		#endregion

		#region 2X
		public static Image SubPixelAntiAliasX2(Image img)
		{
			const int ratio = 2;
			const int shiftSize = 2;
			const uint ratio2 = ratio * ratio;
			Image fImg = new Image(img.Size / ratio);
			Image tmp;
			Vec2 Vec = new Vec2(ratio, ratio);
			Pixel cur;
			uint fY = 0;
			uint fX = 0;

			uint r, g, b, a;

			for (uint y = 0; y < img.Height; y += ratio, fY++)
			{
				fX = 0;
				for (uint x = 0; x < img.Width; x += ratio, fX++)
				{
					tmp = img.SubImage(new Vec2((int)x, (int)y), Vec);
					r = g = b = a = 0;

					for (uint i = 0; i < ratio2; i++)
					{
						cur = tmp.Data[i];
						r += cur.R;
						g += cur.G;
						b += cur.B;
						a += cur.A;
					}

					fImg.SetPixel(fX, fY, new Pixel(
						(byte)(r >> shiftSize),
						(byte)(g >> shiftSize),
						(byte)(b >> shiftSize),
						(byte)(a >> shiftSize)));
				}
			}

			return fImg;
		}
		#endregion

		#region 4X
		public static Image SubPixelAntiAliasX4(Image img)
		{
			const int ratio = 4;
			const int shiftSize = 4;
			const uint ratio2 = ratio * ratio;
			Image fImg = new Image(img.Size / ratio);
			Image tmp;
			Vec2 Vec = new Vec2(ratio, ratio);
			Pixel cur;
			uint fY = 0;
			uint fX = 0;

			uint r, g, b, a;

			for (uint y = 0; y < img.Height; y += ratio, fY++)
			{
				fX = 0;
				for (uint x = 0; x < img.Width; x += ratio, fX++)
				{
					tmp = img.SubImage(new Vec2((int)x, (int)y), Vec);
					r = g = b = a = 0;

					for (uint i = 0; i < ratio2; i++)
					{
						cur = tmp.Data[i];
						r += cur.R;
						g += cur.G;
						b += cur.B;
						a += cur.A;
					}

					fImg.SetPixel(fX, fY, new Pixel(
						(byte)(r >> shiftSize), 
						(byte)(g >> shiftSize),
						(byte)(b >> shiftSize), 
						(byte)(a >> shiftSize)));
				}
			}

			return fImg;
		}
		#endregion

		#region 8X
		public static Image SubPixelAntiAliasX8(Image img)
		{
			const int ratio = 8;
			const int shiftSize = 6;
			const uint ratio2 = ratio * ratio;
			Image fImg = new Image(img.Size / ratio);
			Image tmp;
			Vec2 Vec = new Vec2(ratio, ratio);
			Pixel cur;
			uint fY = 0;
			uint fX = 0;

			uint r, g, b, a;

			for (uint y = 0; y < img.Height; y += ratio, fY++)
			{
				fX = 0;
				for (uint x = 0; x < img.Width; x += ratio, fX++)
				{
					tmp = img.SubImage(new Vec2((int)x, (int)y), Vec);
					r = g = b = a = 0;

					for (uint i = 0; i < ratio2; i++)
					{
						cur = tmp.Data[i];
						r += cur.R;
						g += cur.G;
						b += cur.B;
						a += cur.A;
					}

					fImg.SetPixel(fX, fY, new Pixel(
						(byte)(r >> shiftSize),
						(byte)(g >> shiftSize),
						(byte)(b >> shiftSize),
						(byte)(a >> shiftSize)));
				}
			}

			return fImg;
		}
		#endregion

		#region 16X
		public static Image SubPixelAntiAliasX16(Image img)
		{
			const int ratio = 16;
			const int shiftSize = 8;
			const uint ratio2 = ratio * ratio;
			Image fImg = new Image(img.Size / ratio);
			Image tmp;
			Vec2 Vec = new Vec2(ratio, ratio);
			Pixel cur;
			uint fY = 0;
			uint fX = 0;

			uint r, g, b, a;

			for (uint y = 0; y < img.Height; y += ratio, fY++)
			{
				fX = 0;
				for (uint x = 0; x < img.Width; x += ratio, fX++)
				{
					tmp = img.SubImage(new Vec2((int)x, (int)y), Vec);
					r = g = b = a = 0;

					for (uint i = 0; i < ratio2; i++)
					{
						cur = tmp.Data[i];
						r += cur.R;
						g += cur.G;
						b += cur.B;
						a += cur.A;
					}

					fImg.SetPixel(fX, fY, new Pixel(
						(byte)(r >> shiftSize),
						(byte)(g >> shiftSize),
						(byte)(b >> shiftSize),
						(byte)(a >> shiftSize)));
				}
			}

			return fImg;
		}
		#endregion

		#region 32X
		public static Image SubPixelAntiAliasX32(Image img)
		{
			const int ratio = 32;
			const int shiftSize = 10;
			const uint ratio2 = ratio * ratio;
			Image fImg = new Image(img.Size / ratio);
			Image tmp;
			Vec2 Vec = new Vec2(ratio, ratio);
			Pixel cur;
			uint fY = 0;
			uint fX = 0;

			uint r, g, b, a;

			for (uint y = 0; y < img.Height; y += ratio, fY++)
			{
				fX = 0;
				for (uint x = 0; x < img.Width; x += ratio, fX++)
				{
					tmp = img.SubImage(new Vec2((int)x, (int)y), Vec);
					r = g = b = a = 0;

					for (uint i = 0; i < ratio2; i++)
					{
						cur = tmp.Data[i];
						r += cur.R;
						g += cur.G;
						b += cur.B;
						a += cur.A;
					}

					fImg.SetPixel(fX, fY, new Pixel(
						(byte)(r >> shiftSize),
						(byte)(g >> shiftSize),
						(byte)(b >> shiftSize),
						(byte)(a >> shiftSize)));
				}
			}

			return fImg;
		}
		#endregion

	}
}
