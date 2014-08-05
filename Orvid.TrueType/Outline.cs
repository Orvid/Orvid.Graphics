using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	/// <summary>
	/// Represents a scaled outline.
	/// </summary>
	public class Outline
	{
		/// <summary>
		/// The scaled and translated points in
		/// a contour.
		/// </summary>
		public VecF26Dot6[] Points;
		/// <summary>
		/// An array saying which points are on,
		/// or off, the curve.
		/// </summary>
		public bool[] OnCurve;
		/// <summary>
		/// The end points of the contours.
		/// </summary>
		public int[] CountourEndPoints;
		/// <summary>
		/// This is how far up a rendered
		/// glyph should be shifted when laying
		/// it out, so that (0,0) ends up at
		/// the correct location.
		/// </summary>
		public F26Dot6 Ascent;
		/// <summary>
		/// This is how far down a rendered
		/// glyph should be shifted when laying
		/// it out, so that (0,0) ends up at
		/// the correct location.
		/// </summary>
		public F26Dot6 Descent;
		/// <summary>
		/// This is how far to the right a
		/// rendered glyph should be shifted
		/// when laying it out, so that (0,0)
		/// ends up at the correct location.
		/// </summary>
		public F26Dot6 Advance;
		/// <summary>
		/// This is how far to the left a
		/// rendered glyph should be shifted
		/// when laying it out, so that (0,0)
		/// ends up at the correct location.
		/// </summary>
		public F26Dot6 Devance;
		/// <summary>
		/// The maximum Y position.
		/// </summary>
		public int MaxY;
		/// <summary>
		/// The maximum X position.
		/// </summary>
		public int MaxX;
	}
}
