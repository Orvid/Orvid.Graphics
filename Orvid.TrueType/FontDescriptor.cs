using System;
using System.Collections.Generic;

namespace Orvid.TrueType
{
	/// <summary>
	/// This class contains a set of
	/// methods which describes various
	/// things about this particular font.
	/// </summary>
	public abstract class FontDescriptor
	{
		/// <summary>
		/// Gets the maximum number of points
		/// in a single glyph in the font.
		/// </summary>
		/// <returns>The max number of points.</returns>
		public abstract int GetMaxNumberOfPoints();
		/// <summary>
		/// Gets the max number of twighlight
		/// zone points used in the font.
		/// </summary>
		/// <returns>The max number of twighlight zone points.</returns>
		public abstract int GetMaxNumberOfTwighlightPoints();
		/// <summary>
		/// Gets the max number of contours
		/// in a single glyph in the font.
		/// </summary>
		/// <returns>The max number of contours.</returns>
		public abstract int GetMaxNumberOfContours();
		/// <summary>
		/// Gets the max number of storage
		/// entries used in the font.
		/// </summary>
		/// <returns>The max number of storage entries.</returns>
		public abstract int GetMaxStorage();
		/// <summary>
		/// The initial (unscaled) values for the cvt table.
		/// </summary>
		public int[] CvtValues;

		/// <summary>
		/// The default constructor.
		/// </summary>
		public FontDescriptor()
		{

		}


		private Dictionary<double, GraphicsState> CachedStates = new Dictionary<double, GraphicsState>();
		public GraphicsState GetGraphicsState(double pSize)
		{
#warning Implement Me!
			throw new Exception();
			//return null;
		}

		/// <summary>
		/// Initializes all the values in the graphics state
		/// so that they are valid for this font.
		/// </summary>
		/// <param name="gState">The <see cref="GraphicsState"/> to initialize.</param>
		public void InitializeGraphicsState(GraphicsState gState)
		{
			int maxStorage = GetMaxStorage();
			int maxPoints = GetMaxNumberOfPoints();
			int maxContours = GetMaxNumberOfContours();
			int maxTwilightPoints = GetMaxNumberOfTwighlightPoints();
			gState.Zone1 = new PointZone();
			gState.Zone1.Contour = new VecF26Dot6[maxPoints + 4];
			gState.Zone1.OnCurve = new bool[maxPoints + 4];
			gState.Zone1.OriginalContour = new VecF26Dot6[maxPoints + 4];
			gState.Zone1.OriginalUnscaledContour = new VecF26Dot6[maxPoints + 4];
			gState.Zone1.EndPointsOfContours = new int[maxContours + 4];
			gState.Zone1.TouchedX = new bool[maxPoints + 4];
			gState.Zone1.TouchedY = new bool[maxPoints + 4];
			gState.TwighlightZone = new PointZone();
			gState.TwighlightZone.Contour = new VecF26Dot6[maxTwilightPoints];
			gState.TwighlightZone.PointCount = maxTwilightPoints;
			gState.TwighlightZone.OriginalContour = new VecF26Dot6[maxTwilightPoints];
			gState.Storage = new int[maxStorage];
			gState.Cvt = new F26Dot6[CvtValues.Length];
			for (uint i = 0; i < CvtValues.Length; i++)
			{
				gState.Cvt[i] = F26Dot6.FromDouble(gState.UnitsToPixel(CvtValues[i]));
			}
		}

	}
}
