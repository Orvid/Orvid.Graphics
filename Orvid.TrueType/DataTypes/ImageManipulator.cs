using System;

namespace Orvid.TrueType
{
	public static class ImageManipulator
	{

		#region Flip
		/// <summary>
		/// Flips the specified image vertically.
		/// </summary>
		/// <param name="i">The image to flip.</param>
		/// <returns>The flipped image.</returns>
		public static unsafe Image FlipV(Image img)
		{
			Image o = new Image(img.Size);
			int w = img.Width;
			int iy = img.Height;
			int ix = 0;
			fixed (Pixel* sarr2 = img.Data)
			{
				fixed (Pixel* darr2 = o.Data)
				{
					Pixel* darr = darr2;
					Pixel* sarr = sarr2;
					while (iy >= 0)
					{
						while (ix < w)
						{
							*darr = *sarr;
							sarr++;
							darr++;
							ix++;
						}
						ix = 0;
						iy--;
					}
				}
			}

			return o;
		}
		#endregion

	}
}
