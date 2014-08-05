using System;
using System.Collections.Generic;
using System.Text;
using Orvid.Graphics;

namespace OForms.Mouse.Themes
{
	/// <summary>
	/// The default mouse theme.
	/// </summary>
	public class DefaultMouseTheme : IMouseTheme
	{
		public int MaxHeight { get { return 10; } }
		public int MaxWidth { get { return 10; } }


		private Image local_Default;
		public Image Default { get { return local_Default; } }

		private Image local_HResize;
		public Image HResize { get { return local_HResize; } }

		private Image local_VResize;
		public Image VResize { get { return local_VResize; } }

		private Image local_DLResize;
		public Image DLResize { get { return local_DLResize; } }

		private Image local_DRResize;
		public Image DRResize { get { return local_DRResize; } }

		public DefaultMouseTheme()
		{


			#region Default
			/*
			 * Image: 4x4
			 * 
			 *    x x x -
			 *    x x - -
			 *    x - x -
			 *    - - - x
			 */
			local_Default = new Image(4, 4);
			local_Default.Clear(Colors.Transparent);
			local_Default.SetPixel(0, 0, Colors.Black);
			local_Default.SetPixel(1, 0, Colors.Black);
			local_Default.SetPixel(2, 0, Colors.Black);
			local_Default.SetPixel(0, 1, Colors.Black);
			local_Default.SetPixel(0, 2, Colors.Black);
			local_Default.SetPixel(1, 1, Colors.Black);
			local_Default.SetPixel(2, 2, Colors.Black);
			local_Default.SetPixel(3, 3, Colors.Black);
			#endregion

			#region VResize
			/*
			 * Image: 5x10
			 *    - - x - -
			 *    - x x x -
			 *    x - x - x
			 *    - - x - -
			 *    - - x - -
			 *    - - x - -
			 *    - - x - -
			 *    x - x - x
			 *    - x x x -
			 *    - - x - -
			 */
			local_VResize = new Image(5, 10);
			local_VResize.Clear(Colors.Transparent);
			local_VResize.SetPixel(2, 0, Colors.Black);
			local_VResize.SetPixel(1, 1, Colors.Black);
			local_VResize.SetPixel(2, 1, Colors.Black);
			local_VResize.SetPixel(3, 1, Colors.Black);
			local_VResize.SetPixel(0, 2, Colors.Black);
			local_VResize.SetPixel(2, 2, Colors.Black);
			local_VResize.SetPixel(4, 2, Colors.Black);
			local_VResize.SetPixel(2, 3, Colors.Black);
			local_VResize.SetPixel(2, 4, Colors.Black);
			local_VResize.SetPixel(2, 5, Colors.Black);
			local_VResize.SetPixel(2, 6, Colors.Black);
			local_VResize.SetPixel(0, 7, Colors.Black);
			local_VResize.SetPixel(2, 7, Colors.Black);
			local_VResize.SetPixel(4, 7, Colors.Black);
			local_VResize.SetPixel(1, 8, Colors.Black);
			local_VResize.SetPixel(2, 8, Colors.Black);
			local_VResize.SetPixel(3, 8, Colors.Black);
			local_VResize.SetPixel(2, 9, Colors.Black);
			#endregion

			#region HResize
			/*
			 * Image: 10x5
			 * 
			 *    - - x - - - - x - -
			 *    - x - - - - - - x -
			 *    x x x x x x x x x x
			 *    - x - - - - - - x -
			 *    - - x - - - - x - -
			 */
			local_HResize = new Image(10, 5);
			local_HResize.Clear(Colors.Transparent);
			local_HResize.SetPixel(2, 0, Colors.Black);
			local_HResize.SetPixel(7, 0, Colors.Black);
			local_HResize.SetPixel(1, 1, Colors.Black);
			local_HResize.SetPixel(8, 1, Colors.Black);
			local_HResize.SetPixel(0, 2, Colors.Black);
			local_HResize.SetPixel(1, 2, Colors.Black);
			local_HResize.SetPixel(2, 2, Colors.Black);
			local_HResize.SetPixel(3, 2, Colors.Black);
			local_HResize.SetPixel(4, 2, Colors.Black);
			local_HResize.SetPixel(5, 2, Colors.Black);
			local_HResize.SetPixel(6, 2, Colors.Black);
			local_HResize.SetPixel(7, 2, Colors.Black);
			local_HResize.SetPixel(8, 2, Colors.Black);
			local_HResize.SetPixel(9, 2, Colors.Black);
			local_HResize.SetPixel(1, 3, Colors.Black);
			local_HResize.SetPixel(8, 3, Colors.Black);
			local_HResize.SetPixel(2, 4, Colors.Black);
			local_HResize.SetPixel(7, 4, Colors.Black);
			#endregion

			#region DLResize
			/*
			 * Image: 7x7
			 * 
			 *    - - - - x x x
			 *    - - - - - x x
			 *    - - - - x - x
			 *    - - - x - - -
			 *    x - x - - - -
			 *    x x - - - - -
			 *    x x x - - -  -
			 */
			local_DLResize = new Image(7, 7);
			local_DLResize.Clear(Colors.Transparent);
			local_DLResize.SetPixel(4, 0, Colors.Black);
			local_DLResize.SetPixel(5, 0, Colors.Black);
			local_DLResize.SetPixel(6, 0, Colors.Black);
			local_DLResize.SetPixel(5, 1, Colors.Black);
			local_DLResize.SetPixel(6, 1, Colors.Black);
			local_DLResize.SetPixel(4, 2, Colors.Black);
			local_DLResize.SetPixel(6, 2, Colors.Black);
			local_DLResize.SetPixel(3, 3, Colors.Black);
			local_DLResize.SetPixel(0, 4, Colors.Black);
			local_DLResize.SetPixel(2, 4, Colors.Black);
			local_DLResize.SetPixel(0, 5, Colors.Black);
			local_DLResize.SetPixel(1, 5, Colors.Black);
			local_DLResize.SetPixel(0, 6, Colors.Black);
			local_DLResize.SetPixel(1, 6, Colors.Black);
			local_DLResize.SetPixel(2, 6, Colors.Black);
			#endregion

			#region DRResize
			/*
			 * Image: 7x7
			 * 
			 *    x x x - - - -
			 *    x x - - - - -
			 *    x - x - - - -
			 *    - - - x - - -
			 *    - - - - x - x 
			 *    - - - - - x x
			 *    - - - - x x x 
			 */
			local_DRResize = new Image(7, 7);
			local_DRResize.Clear(Colors.Transparent);
			local_DRResize.SetPixel(0, 0, Colors.Black);
			local_DRResize.SetPixel(1, 0, Colors.Black);
			local_DRResize.SetPixel(2, 0, Colors.Black);
			local_DRResize.SetPixel(0, 1, Colors.Black);
			local_DRResize.SetPixel(1, 1, Colors.Black);
			local_DRResize.SetPixel(0, 2, Colors.Black);
			local_DRResize.SetPixel(2, 2, Colors.Black);
			local_DRResize.SetPixel(3, 3, Colors.Black);
			local_DRResize.SetPixel(4, 4, Colors.Black);
			local_DRResize.SetPixel(6, 4, Colors.Black);
			local_DRResize.SetPixel(5, 5, Colors.Black);
			local_DRResize.SetPixel(6, 5, Colors.Black);
			local_DRResize.SetPixel(4, 6, Colors.Black);
			local_DRResize.SetPixel(5, 6, Colors.Black);
			local_DRResize.SetPixel(6, 6, Colors.Black);
			#endregion


		}


	}
}
