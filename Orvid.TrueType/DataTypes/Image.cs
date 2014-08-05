using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	/// <summary>
	/// This class is used to describe an image.
	/// </summary>
	public class Image
	{
		/// <summary>
		/// The raw data in the image.
		/// </summary>
		public Pixel[] Data;
		private int local_X;
		private int local_Y;

		/// <summary>
		/// The size of the image.
		/// </summary>
		public Vec2 Size
		{
			get { return new Vec2(local_X, local_Y); }
		}

		/// <summary>
		/// The height of the image.
		/// </summary>
		public int Height
		{
			get { return local_Y; }
		}
		/// <summary>
		/// The width of the image.
		/// </summary>
		public int Width
		{
			get { return local_X; }
		}

		/// <summary>
		/// Creates a new image with the specified height and width.
		/// </summary>
		/// <param name="height">The height of the new image.</param>
		/// <param name="width">The width of the new image.</param>
		public Image(int width, int height)
		{
			this.local_X = width;
			this.local_Y = height;
			this.Data = new Pixel[Height * Width];
		}

		public Image(Vec2 size) : this(size.X, size.Y) { }

		#region Clear
		public void Clear(Pixel color)
		{
			for (uint i = 0; i < Data.Length; i++)
			{
				Data[i] = color;
			}
		}
		#endregion

		#region SubImage
		/// <summary>
		/// Gets a sub-image of this image,
		/// from the specified location,
		/// of the specified size.
		/// </summary>
		/// <param name="loc">The location of the image to pull.</param>
		/// <param name="size">The size of the image to pull.</param>
		/// <returns>The sub-image obtained.</returns>
		public Image SubImage(Vec2 loc, Vec2 size)
		{
#warning Need to do an unsafe version of this, because this is SLOW
			Image i = new Image(size);
			for (int y = loc.Y; y < (loc.Y + size.Y); y++)
			{
				for (int x = loc.X; x < (loc.X + size.X); x++)
				{
					i.SetPixel((uint)(x - loc.X), (uint)(y - loc.Y), this.GetPixel((uint)x, (uint)y));
				}
			}
			return i;
		}
		#endregion

		/// <summary>
		/// Get's the pixel a the specified location.
		/// </summary>
		/// <param name="width">The width position of the pixel.</param>
		/// <param name="height">The height position of the pixel.</param>
		/// <returns>The Pixel at the specified position.</returns>
		public Pixel GetPixel(uint x, uint y)
		{
			if (/*x > 0 &&*/ x < Width /*&& y > 0 */&& y < Height)
				return Data[(y * Width) + x];
			else
				return new Pixel(0, 0, 0, 0);
		}

		/// <summary>
		/// Set's the pixel at the specified location, 
		/// to the specified pixel.
		/// </summary>
		/// <param name="width">The width position of the pixel.</param>
		/// <param name="height">The height position of the pixel.</param>
		/// <param name="p">The pixel to set to.</param>
		public void SetPixel(uint x, uint y, Pixel p)
		{
			if (p.A != 255)
			{
				if ((p.A == 1) && (p.R + p.G + p.B == 0))
				{
					// This allows us to empty pixels.
					//throw new Exception();
					Data[((y * Width) + x)] = p;
				}
				else if (p.A != 0)
				{
					double r1 = ((double)p.A / 255);
					Pixel cur = Data[((y * Width) + x)];
					double r2 = 1.0d - r1;

					Data[((y * Width) + x)] = new Pixel(
						(byte)((p.R * r1) + (cur.R * r2)),
						(byte)((p.G * r1) + (cur.G * r2)),
						(byte)((p.B * r1) + (cur.B * r2)),
						255
						);
				}
				// else nothing gets drawn.
			}
			else
			{
				Data[((y * Width) + x)] = p;
			}
		}

	}
}
