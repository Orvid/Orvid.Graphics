using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Orvid.TrueType
{
	/// <summary>
	/// Represents a single glyph,
	/// and can do all hinting, and
	/// rendering of itself.
	/// </summary>
	/// <remarks>
	/// All fields in the this class
	/// are in FUnits, and thus, are
	/// independant of both point size, 
	/// and the DPI of the screen.
	/// </remarks>
	public abstract class Glyph
	{
		/// <summary>
		/// The contour of this glyph.
		/// </summary>
		public Vec2[] GlyphContour;
		/// <summary>
		/// An array representing which
		/// points are on a curve.
		/// </summary>
		public bool[] PointsOnCurve;
		/// <summary>
		/// An array representing the end
		/// points of each contour.
		/// </summary>
		public int[] EndPointsOfCountours;
		/// <summary>
		/// The distance above the baseline
		/// to render the base of this glyph.
		/// </summary>
		public int Ascender;
		/// <summary>
		/// The distance below the baseline
		/// to render the base of this glyph.
		/// </summary>
		public int Descender;
		/// <summary>
		/// The ammount of space that is left
		/// on the left side of this glyph.
		/// </summary>
		public int LeftSideBearing;
		/// <summary>
		/// The ammount of space that is left
		/// on the right side of this glyph.
		/// </summary>
		public int RightSideBearing;
		/// <summary>
		/// The distance to advance from the
		/// start of this glyph before rendering
		/// the next glyph.
		/// </summary>
		public int AdvanceWidth;
		/// <summary>
		/// The initial x location of phantom
		/// point 1.
		/// </summary>
		public int PhantomPoint1;
		/// <summary>
		/// The initial x location of phantom
		/// point 2.
		/// </summary>
		public int PhantomPoint2;

		/// <summary>
		/// Executes the hinting of this glyph.
		/// </summary>
		/// <param name="g">The graphics state to hint.</param>
		public abstract void Hint(GraphicsState g);

		/// <summary>
		/// Initializes the specified <see cref="GraphicsState"/>
		/// with the data for this specific glyph.
		/// </summary>
		/// <param name="g">The <see cref="GraphicsState"/> to initialize.</param>
		public void InitializeGraphicsState(GraphicsState g)
		{
			CopyUnscaled(g.Zone1.OriginalUnscaledContour, GlyphContour);
			ResizeAndCopyToContour(g.Zone1.OriginalContour, GlyphContour, g);
			// Add the points for LSB and RSB.
			g.Zone1.OriginalContour[GlyphContour.Length] = new VecF26Dot6(g.UnitsToPixel(PhantomPoint1), 0);
			g.Zone1.OriginalContour[GlyphContour.Length + 1] = new VecF26Dot6(g.UnitsToPixel(PhantomPoint2), 0);
			// These next 2 would be vertical origin metrics.
			//g.Zone1.OriginalContour[GlyphContour.Length + 2] = new VecF26Dot6(g.UnitsToPixel(Ascender), 0); 
			//g.Zone1.OriginalContour[GlyphContour.Length + 3] = new VecF26Dot6(g.UnitsToPixel(Descender), 0);
			Array.Copy(g.Zone1.OriginalContour, g.Zone1.Contour, g.Zone1.OriginalContour.Length);
			Array.Copy(PointsOnCurve, g.Zone1.OnCurve, PointsOnCurve.Length);
			Array.Copy(EndPointsOfCountours, g.Zone1.EndPointsOfContours, EndPointsOfCountours.Length);
			g.Zone1.ContourCount = this.EndPointsOfCountours.Length;
			g.Zone1.PointCount = this.GlyphContour.Length;
		}

		/// <summary>
		/// Copies the unscaled outline in.
		/// </summary>
		private static unsafe void CopyUnscaled(VecF26Dot6[] darr3, Vec2[] sarr3)
		{
			int len = sarr3.Length;
			int i = 0;
			fixed (VecF26Dot6* darr2 = darr3)
			{
				fixed (Vec2* sarr2 = sarr3)
				{
					VecF26Dot6* darr = darr2;
					Vec2* sarr = sarr2;
					while (i < len)
					{
						*darr = new VecF26Dot6(sarr->X, sarr->Y);
						sarr++;
						darr++;
						i++;
					}
				}
			}
		}

		/// <summary>
		/// Scales the contour points, and copies them
		/// into the destination array.
		/// </summary>
		/// <param name="pDarr">The array to put the scaled points in.</param>
		/// <param name="pSarr">The array of the source contour points.</param>
		/// <param name="g">
		/// The graphics state which contains information
		/// such as the point size we're rendering at.
		/// </param>
		/// <remarks>
		/// We use unsafe code here rather than a direct array
		/// copy because we'd be modifying the points anyways,
		/// thus this is much faster than repeated array lookups.
		/// </remarks>
		private static unsafe void ResizeAndCopyToContour(VecF26Dot6[] pDarr, Vec2[] pSarr, GraphicsState g)
		{
			fixed (VecF26Dot6* darr2 = pDarr)
			{
				fixed (Vec2* sarr2 = pSarr)
				{
					Vec2* sarr = sarr2;
					VecF26Dot6* darr = darr2;
					//System.Runtime.InteropServices.
					//((byte*)sarr)darr();
					int len = pSarr.Length * sizeof(VecF26Dot6);
					while ((int)sarr - (int)sarr2 < len)
					{
						*darr = new VecF26Dot6(
							F26Dot6.FromDouble(g.UnitsToPixel((*sarr).X)),
							F26Dot6.FromDouble(g.UnitsToPixel((*sarr).Y))
						);
						darr++;
						sarr++;
					}
				}
			}
		}

		/// <summary>
		/// The cached versions of the outlines of this glyph.
		/// This is keyed by the point size.
		/// </summary>
		private Dictionary<double, Outline> CachedOutlines = new Dictionary<double, Outline>();
		/// <summary>
		/// The cached versions of the rendered versions of this
		/// glyph. This is keyed by the point size.
		/// </summary>
		private Dictionary<double, Image> CachedImages = new Dictionary<double, Image>();

		public Image CachedRender(double pointSize)
		{
#warning Implement Me
			throw new Exception();
			//if (CachedImages.ContainsKey(pointSize))
			//{
			//    return CachedImages[pointSize];
			//}
			//else if (CachedOutlines.ContainsKey(pointSize))
			//{
			//    // The outline cache keeps it slightly deeper than the image cache.
			//    return null;
			//}
			//else
			//{
			//    // The glyph isn't cached at all.
			//    return null;
			//}
		}

		#region Rendering

		public const int CharacterSide = 0;
		// 4 is the only real stable ratio in this. (other than 1 of course)
		private const int SizeRatio = 1;
		private const bool UseSubPixel = false;
		private const bool ClearFill = true;
		private const bool RenderFill = true;
		private const bool RenderPoints = false;
		//private static readonly Pixel FillColor = Colors.Black;
		//private static readonly Pixel ClearColor = Colors.White;
		private static readonly Pixel PointColor = Colors.Green;
		private static readonly Pixel FillColor = Colors.Red;
		private static readonly Pixel ClearColor = Colors.Black;
		private const int XSizeRatio = UseSubPixel ? 3 : 1;

		#region Get Contour Points
		private static Vec2d[] GetContourPoints(uint contourIndex, double SizeInPoints, GraphicsState gState)
		{
			List<Vec2d[]> Curves = new List<Vec2d[]>(300);
			int pIndex = (contourIndex >= 1) ? gState.Zone1.EndPointsOfContours[contourIndex - 1] + 1 : 0;
			int eIndex = gState.Zone1.EndPointsOfContours[contourIndex];
			Vec2d[] FinalPoints = new Vec2d[gState.Zone1.EndPointsOfContours[contourIndex] + 1 - pIndex];
			int state = 0;
			Vec2d pA = Vec2d.Zero;
			Vec2d pB = Vec2d.Zero;
			Vec2d pC = Vec2d.Zero;
			for (uint i = 0; pIndex < gState.Zone1.EndPointsOfContours[contourIndex] + 1; i++, pIndex++)
			{
				FinalPoints[i] = new Vec2d(
					((F26Dot6.ToDouble(gState.Zone1.Contour[pIndex].X)) * SizeRatio * XSizeRatio),
					((F26Dot6.ToDouble(gState.Zone1.Contour[pIndex].Y)) * SizeRatio)
				);
				switch (state)
				{
					case 0:
						if (!gState.Zone1.OnCurve[pIndex])
						{
							if (!gState.Zone1.OnCurve[eIndex])
							{
								FinalPoints[eIndex - pIndex] = new Vec2d(
									(double)(int)((F26Dot6.ToDouble(gState.Zone1.Contour[eIndex].X)) * SizeRatio * XSizeRatio),
									(double)(int)((F26Dot6.ToDouble(gState.Zone1.Contour[eIndex].Y)) * SizeRatio)
								);
								pB = FinalPoints[i];
								pA = (FinalPoints[eIndex - pIndex] + pB) / 2;
								//Console.WriteLine("State is 0 and pA = (" + pA.ToString() + "), pB = (" + pB.ToString() + ") Advancing to state 2");
								state = 2;
							}
							else
							{
								FinalPoints[eIndex - pIndex] = new Vec2d(
									(double)(int)((F26Dot6.ToDouble(gState.Zone1.Contour[eIndex].X)) * SizeRatio * XSizeRatio),
									(double)(int)((F26Dot6.ToDouble(gState.Zone1.Contour[eIndex].Y)) * SizeRatio)
								);
								pA = FinalPoints[eIndex - pIndex];
								pB = FinalPoints[i];
								//Console.WriteLine("State is 0 and pA = (" + pA.ToString() + "), pB = (" + pB.ToString() + ") Advancing to state 2");
								state = 2;
							}
						}
						else
						{
							pA = FinalPoints[i];
							//Console.WriteLine("State is 0 and pA = (" + pA.ToString() + ") Advancing to state 1");
							state = 1;
						}
						break;
					case 1:
						if (!gState.Zone1.OnCurve[pIndex])
						{
							pB = FinalPoints[i];
							//Console.WriteLine("State is 1 and pA = (" + pA.ToString() + "), pB = (" + pB.ToString() + ") Advancing to state 2");
							state = 2;
						}
						else
						{
							pB = FinalPoints[i];
							//Console.WriteLine("State is 1 and pA = (" + pA.ToString() + "), pB = (" + pB.ToString() + ") Adding curve.");
							Curves.Add(new Vec2d[] { pA, pB });
							pA = pB;
						}
						break;
					case 2:
						if (!gState.Zone1.OnCurve[pIndex])
						{
							pC = (FinalPoints[i] + pB) / 2;
							//Console.WriteLine("State is 2 and pA = (" + pA.ToString() + "), pB = (" + pB.ToString() + "), and pC = (" + pC.ToString() + ") Staying at state 2 and Adding Curve");
							Curves.Add(new Vec2d[] { pA, pB, pC });
							pA = pC;
							pB = FinalPoints[i];
						}
						else
						{
							pC = FinalPoints[i];
							//Console.WriteLine("State is 2 and pA = (" + pA.ToString() + "), pB = (" + pB.ToString() + "), and pC = (" + pC.ToString() + ") Returning to state 1 and Adding Curve");
							Curves.Add(new Vec2d[] { pA, pB, pC });
							pA = pC;
							state = 1;
						}
						break;
					default:
						throw new Exception("Unknown state!");
				}
			}

			switch (state)
			{
				case 1:
					Curves.Add(new Vec2d[] { pA, FinalPoints[0] });
					break;
				case 2:
					Curves.Add(new Vec2d[] { pA, pB, FinalPoints[0] });
					break;
				default:
					throw new Exception("Invalid state!");
			}
			List<Vec2d> Expanded = new List<Vec2d>(4096);
			//System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
			foreach (Vec2d[] v in Curves)
			{
				//st.Start();
				//Vec2d[] expArr = BezierPointExpansion.ExpandArbitraryBezier(v);
				//st.Stop();
				//Vec2[] FilteredCurve = FilterBezierCurve(expArr);
				//Expanded.AddRange(FilteredCurve);
				Expanded.AddRange(FilterBezierCurve(BezierPointExpansion.ExpandArbitraryBezier(v)));
			}
			//Console.WriteLine("Curve expansion took " + st.ElapsedTicks + " ticks (" + st.ElapsedMilliseconds.ToString() + "ms).");
			return Expanded.ToArray();
		}

		/// <summary>
		/// This essentially removes duplicate points from
		/// a generated bezier curve.
		/// </summary>
		/// <remarks>
		/// This utalizes the fact that duplicate points will
		/// be next to each other in the array to filter the
		/// points faster.
		/// This also does implicit truncation of the Vec2d's
		/// into Vec2's.
		/// </remarks>
		private static unsafe Vec2d[] FilterBezierCurve(Vec2d[] ExpandedPoints)
		{
			Vec2d[] dst = new Vec2d[ExpandedPoints.Length];
			Vec2d[] FinalPoints;
			Vec2d lPt = ExpandedPoints[0];
			fixed (Vec2d* sarr2 = ExpandedPoints)
			{
				fixed (Vec2d* dpts2 = dst)
				{
					Vec2d* dpts = dpts2;
					Vec2d* sarr = sarr2;
					int len = ExpandedPoints.Length;
					int i = 0;
					while (i < len)
					{
						if ((Vec2d)((VecF26Dot6)(*sarr)) != lPt)
						{
							*dpts = (Vec2d)((VecF26Dot6)(*sarr));
							lPt = (Vec2d)((VecF26Dot6)(*sarr));
							dpts++;
						}
						sarr++;
						i++;
					}
					int pointCount = (int)(dpts - dpts2);
					FinalPoints = new Vec2d[pointCount];
					Array.Copy(dst, FinalPoints, pointCount);
				}
			}
			return FinalPoints;
		}
		#endregion

		private static unsafe Image WhiteToTransparent(Image img)
		{
			// This isn't a very noticable
			// difference vs. the safe version
			// except when you get into the 
			// higher point range, such as 600pt
			Image o = new Image(img.Size);
			uint len = (uint)(img.Data.Length * sizeof(Pixel));
			fixed (Pixel* pArrOrg = img.Data)
			{
				fixed (Pixel* pArrNew = o.Data)
				{
					int* parr = (int*)(void*)pArrOrg;
					int* parr2 = (int*)(void*)pArrNew;
					Pixel tr = Colors.Transparent;
					int white = -1; // huzzah for knowledge!
					int trans = *(int*)(void*)&tr;
					while ((uint)parr - (uint)pArrOrg < len)
					{
						if (*parr == white)
							*parr2 = trans;
						else
							*parr2 = *parr;
						parr++;
						parr2++;
					}
				}
			}
			return o;
		}

		
		//private enum LineDirection : byte
		//{
		//    Ascending,
		//    Descending,
		//}

		//[StructLayout(LayoutKind.Explicit)]
		//private struct ContourSegment
		//{
		//    [FieldOffset(0)]
		//    public Vec2d PointA;
		//    [FieldOffset(8)]
		//    public Vec2d PointB;
		//    [FieldOffset(16)]
		//    public LineDirection Direction;

		//    public ContourSegment(Vec2d pA, Vec2d pB, LineDirection dir)
		//    {
		//        this.PointA = pA;
		//        this.PointB = pB;
		//        this.Direction = dir;
		//    }

		//    public override string ToString()
		//    {
		//        return (Direction.ToString()) + " from (" + PointA.ToString() + ") to (" + PointB.ToString() + ")";
		//    }
		//}

		//[StructLayout(LayoutKind.Explicit)]
		//private struct ScanlinePoint
		//{
		//    [FieldOffset(0)]
		//    public double XPosition;
		//    [FieldOffset(8)]
		//    public LineDirection Flags;

		//    public ScanlinePoint(double pX, LineDirection flags)
		//    {
		//        this.XPosition = pX;
		//        this.Flags = flags;
		//    }

		//    public override string ToString()
		//    {
		//        return Flags.ToString() + " at " + XPosition.ToString();
		//    }
		//}

		//private struct Scanline
		//{
		//    /// <summary>
		//    /// This is a list of points this scanline
		//    /// hits. This is null after the array is set.
		//    /// </summary>
		//    public List<ScanlinePoint> PointList;
		//    public ScanlinePoint[] Points;
		//    public int YPosition;
		//}
		
		public unsafe Image GetRendering(double SizeInPoints, GraphicsState gState)
		{
			//LinkedStack<Vec2[]> cPoints = new LinkedStack<Vec2[]>();
			//System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
			//st.Start();
			//for (uint i = 0; i < this.EndPointsOfCountours.Length; i++)
			//{
			//    cPoints.Push(GetContourPoints(i, SizeInPoints, gState));
			//}
			//st.Stop();
			//Console.WriteLine("Contour point retrieval took " + st.ElapsedTicks.ToString() + " ticks (" + st.ElapsedMilliseconds.ToString() + "ms) to run.");
			//st.Reset();

			//List<Vec2d[]> Polygons = new List<Vec2d[]>(gState.Zone1.ContourCount);
			List<Vec2d[]> SolidPolygons = new List<Vec2d[]>(gState.Zone1.ContourCount);
			List<Vec2d[]> EmptyPolygons = new List<Vec2d[]>(gState.Zone1.ContourCount);
			for (uint i = 0; i < this.EndPointsOfCountours.Length; i++)
			{
				//Polygons.Add(GetContourPoints(i, SizeInPoints, gState));
				Vec2d[] tmp = GetContourPoints(i, SizeInPoints, gState);
				int i2 = 1;
				while (tmp[i2].Y == tmp[i2 - 1].Y)
					i2++;
				if (tmp[i2].Y < tmp[i2 - 1].Y)
					EmptyPolygons.Add(tmp);
				else
					SolidPolygons.Add(tmp);
			}
			double MaxX = 0, MaxY = 0, MinX = 0, MinY = 0;
			//GetMinAndMax(Polygons, ref MaxX, ref MaxY, ref MinX, ref MinY);
			GetMinAndMax(EmptyPolygons, ref MaxX, ref MaxY, ref MinX, ref MinY);
			GetMinAndMax(SolidPolygons, ref MaxX, ref MaxY, ref MinX, ref MinY);

			Image img = new Image((int)(Math.Ceiling(MaxX) - Math.Floor(MinX)) + (SizeRatio * CharacterSide) + 1, (int)(Math.Ceiling(MaxY) - Math.Floor(MinY)) + (SizeRatio * CharacterSide) + 1);

			// Now we decompose it into scanlines.... Fun.

			//ContourSegment[] cSegs;
			//{
			//    List<ContourSegment> segs = new List<ContourSegment>();
			//    foreach (Vec2d[] varr in Polygons)
			//    {
			//        int i = 0;
			//        int len = varr.Length;
			//        fixed (Vec2d* pts = varr)
			//        {
			//            Vec2d* p1 = pts;
			//            Vec2d* p2 = &pts[len - 1];
			//            while (i < len)
			//            {
			//                if (p1->Y > p2->Y)
			//                {
			//                    segs.Add(new ContourSegment(*p1, *p2, LineDirection.Descending));
			//                }
			//                else if (p1->Y < p2->Y)
			//                {
			//                    segs.Add(new ContourSegment(*p1, *p2, LineDirection.Ascending));
			//                }
			//                else
			//                {
			//                    //segs.Add(new ContourSegment(*p1, *p2, LineDirection.None));
			//                }
			//                p2 = p1++;
			//                i++;
			//            }
			//        }
			//    }
			//    cSegs = segs.ToArray();
			//    segs = null;
			//}

			//Scanline[] scans = new Scanline[img.Height];
			//for (int i = 0; i < scans.Length; i++)
			//{
			//    scans[i].PointList = new List<ScanlinePoint>();
			//    scans[i].YPosition = i;
			//}
			//fixed (ContourSegment* cSegs3 = cSegs)
			//{
			//    ContourSegment* seg = cSegs3;
			//    int len = cSegs.Length;
			//    int i = 0;
			//    int yPos = 0;
			//    double rise = 0;
			//    while (i < len)
			//    {
			//        switch (seg->Direction)
			//        {
			//            case LineDirection.Ascending:
			//                rise = seg->PointB.Y - seg->PointA.Y;
			//                if (rise < 1)
			//                {
			//                    (seg + 1)->PointA = seg->PointA;
			//                    seg++;
			//                    i++;
			//                    continue;
			//                }
			//                else if (rise > 1.5)
			//                {
			//                    // We need to do a linear interpolation of positions.
			//                    int pCount = (int)Math.Round(rise) - 1;
			//                    if (seg->PointA.X == seg->PointB.X || Math.Abs(seg->PointA.X - seg->PointB.X) < 1)
			//                    {
			//                        int baseYPos = (int)Math.Round(seg->PointA.Y);
			//                        for (int i2 = 0; i2 < pCount; i2++, baseYPos++)
			//                        {
			//                            scans[baseYPos].PointList.Add(new ScanlinePoint(seg->PointA.X, seg->Direction));
			//                        }
			//                        yPos = baseYPos;
			//                    }
			//                    else
			//                    {
			//                        double slope = (seg->PointA.Y - seg->PointB.Y) / (seg->PointA.X - seg->PointB.X);
			//                        int baseYPos = (int)Math.Round(seg->PointA.Y);
			//                        for (int i2 = 0; i2 < pCount; i2++, baseYPos++)
			//                        {
			//                            scans[baseYPos].PointList.Add(new ScanlinePoint(seg->PointA.X + (i2 * slope), seg->Direction));
			//                        }
			//                        yPos = baseYPos;
			//                    }
			//                }
			//                else
			//                {
			//                    yPos = (int)Math.Round(seg->PointA.Y + (rise / 2));
			//                }
			//                break;
			//            case LineDirection.Descending:
			//                rise = seg->PointA.Y - seg->PointB.Y;
			//                if (rise < 1)
			//                {
			//                    (seg + 1)->PointA = seg->PointA;
			//                    seg++;
			//                    i++;
			//                    continue;
			//                }
			//                else if (rise > 1.5)
			//                {
			//                    // We need to do a linear interpolation of positions.
			//                    int pCount = (int)Math.Round(rise) - 1;
			//                    if (seg->PointA.X == seg->PointB.X || Math.Abs(seg->PointA.X - seg->PointB.X) < 1)
			//                    {
			//                        int baseYPos = (int)Math.Round(seg->PointA.Y);
			//                        for (int i2 = 0; i2 < pCount; i2++, baseYPos--)
			//                        {
			//                            scans[baseYPos].PointList.Add(new ScanlinePoint(seg->PointA.X, seg->Direction));
			//                        }
			//                        yPos = baseYPos;
			//                    }
			//                    else
			//                    {
			//                        double slope = (seg->PointA.Y - seg->PointB.Y) / (seg->PointA.X - seg->PointB.X);
			//                        int baseYPos = (int)Math.Round(seg->PointA.Y);
			//                        for (int i2 = 0; i2 < pCount; i2++, baseYPos--)
			//                        {
			//                            scans[baseYPos].PointList.Add(new ScanlinePoint(seg->PointA.X - (i2 * slope), seg->Direction));
			//                        }
			//                        yPos = baseYPos;
			//                    }
			//                }
			//                else
			//                {
			//                    yPos = (int)Math.Round(seg->PointA.Y - (rise / 2));
			//                }
			//                break;
			//            //case LineDirection.None:
			//            //    seg++;
			//            //    i++;
			//            //    continue;
			//        }
			//        double w = seg->PointA.X - seg->PointB.X;
			//        if (w < 0)
			//        {
			//            w = -w;
			//        }
			//        if (w > 1)
			//        {
			//            if (seg->Direction == LineDirection.Ascending)
			//            {
			//                scans[yPos].PointList.Add(new ScanlinePoint(seg->PointA.X, LineDirection.Ascending));
			//                scans[yPos].PointList.Add(new ScanlinePoint(seg->PointB.X, LineDirection.Descending));
			//            }
			//            else
			//            {
			//                scans[yPos].PointList.Add(new ScanlinePoint(seg->PointA.X, LineDirection.Descending));
			//                scans[yPos].PointList.Add(new ScanlinePoint(seg->PointB.X, LineDirection.Ascending));
			//            }
			//        }
			//        else if (w == 0)
			//        {
			//            scans[yPos].PointList.Add(new ScanlinePoint(seg->PointA.X, seg->Direction));
			//        }
			//        else
			//        {
			//            // otherwise average the 2.
			//            scans[yPos].PointList.Add(new ScanlinePoint((seg->PointA.X + seg->PointB.X) / 2, seg->Direction));
			//        }
			//        i++;
			//        seg++;
			//    }
			//}

			// It complains about un-reachable code due
			// to most of this being dependant on the value
			// of a set of constants.
#pragma warning disable 162
			if (ClearFill)
				img.Clear(ClearColor);

			Vec2d Offset = new Vec2d(-(MinX - 0.5), -(MinY - 0.5));
			//for (int i = 0; i < scans.Length; i++)
			//{
			//    scans[i].PointList.Sort(new Comparison<ScanlinePoint>(delegate(ScanlinePoint p1, ScanlinePoint p2)
			//    {
			//        return p1.XPosition.CompareTo(p2.XPosition);
			//    }));
			//    scans[i].Points = scans[i].PointList.ToArray();
			//    scans[i].PointList = null;
			//}

			//foreach (Scanline sc in scans)
			//{
			//    List<ScanlinePoint> ascending = new List<ScanlinePoint>(), descending = new List<ScanlinePoint>();
			//    foreach (ScanlinePoint scp in sc.Points)
			//    {
			//        if (scp.Flags == LineDirection.Ascending)
			//            ascending.Add(scp);
			//        else
			//            descending.Add(scp);
			//    }
			//    if (ascending.Count > 0 && descending.Count > 0)
			//    {
			//        int ei = 1;
			//        int si = 1;
			//        double xEPos = descending[0].XPosition + Offset.X;
			//        double xSPos = ascending[0].XPosition + Offset.X;
			//        bool Solid = false;
			//        for (uint i = 0; i < img.Width; i++)
			//        {
			//            if (i >= xSPos)
			//            {
			//                if (si <= ascending.Count)
			//                {
			//                    if (si == ascending.Count)
			//                    {
			//                        break;
			//                    }
			//                    Solid = true;
			//                    while (i >= xSPos && si < ascending.Count)
			//                    {
			//                        xSPos = ascending[si].XPosition + Offset.X;
			//                        si++;
			//                    }
			//                }
			//            }
			//            if (i >= xEPos)
			//            {
			//                if (ei <= descending.Count)
			//                {
			//                    if (ei == descending.Count)
			//                    {
			//                        break;
			//                    }
			//                    Solid = false;
			//                    while (i >= xEPos && ei < descending.Count)
			//                    {
			//                        xEPos = descending[ei].XPosition + Offset.X;
			//                        ei++;
			//                    }
			//                }
			//            }
			//            if (Solid)
			//            {
			//                img.SetPixel(i, (uint)sc.YPosition, FillColor);
			//            }
			//        }
			//        //for (int i = 0; i < ascending.Count; i++)
			//        //{
			//        //    for (uint i2 = xSPos; i2 < xEPos + 1; i2++)
			//        //    {
			//        //        if (i2 > ascending[i].XPosition + Offset.X && i2 < descending[i].XPosition + Offset.X)
			//        //        {
			//        //            img.SetPixel(i2, (uint)sc.YPosition, FillColor);
			//        //        }
			//        //    }
			//        //}
			//    }
			//}

			if (RenderFill)
			{
				fixed (Pixel* dImg2 = img.Data)
				{
					Pixel* dImg = dImg2;
					int w = img.Width;
					int len = img.Height * w;
					Vec2d curPos = Vec2d.Zero;
					int i = 0;
					while (i < len)
					{
						if (curPos.X >= w)
						{
							curPos.X = 0;
							curPos.Y++;
						}
						bool inPoly = false;

						foreach (Vec2d[] varr in SolidPolygons)
						{
							if (PointInPolygon(varr, curPos - Offset))
							{
								inPoly = true;
								break;
							}
						}
						if (inPoly)
						{
							foreach (Vec2d[] varr in EmptyPolygons)
							{
								if (PointInPolygon(varr, curPos - Offset))
								{
									inPoly = false;
									break;
								}
							}
						}
						if (inPoly)
						{
							*dImg = FillColor;
						}
						curPos.X++;
						i++;
						dImg++;
					}
				}
			}

			if (RenderPoints)
			{
				foreach (Vec2d[] varr in SolidPolygons)
				{
					foreach (Vec2d v in varr)
					{
						img.SetPixel((uint)(v.X + Offset.X), (uint)(v.Y + Offset.Y), PointColor);
					}
				}
				foreach (Vec2d[] varr in EmptyPolygons)
				{
					foreach (Vec2d v in varr)
					{
						img.SetPixel((uint)(v.X + Offset.X), (uint)(v.Y + Offset.Y), PointColor);
					}
				}
			}

			if (UseSubPixel)
			{
				img = SubPixelResize.Resize(img);
			}
			if (SizeRatio > 1)
			{
				switch (SizeRatio)
				{
					case 2:
						img = SubPixelResize.SubPixelAntiAliasX2(img);
						break;
					case 4:
						img = SubPixelResize.SubPixelAntiAliasX4(img);
						break;
					case 8:
						img = SubPixelResize.SubPixelAntiAliasX8(img);
						break;
					case 16:
						img = SubPixelResize.SubPixelAntiAliasX16(img);
						break;
					case 32:
						img = SubPixelResize.SubPixelAntiAliasX32(img);
						break;

					default: break;
				}
			}

			img = ImageManipulator.FlipV(img);

			if (UseSubPixel)
			{
				return WhiteToTransparent(img);
			}
			else
			{
				return img;
			}
#pragma warning restore 162
		}

		private static unsafe void GetMinAndMax(List<Vec2d[]> arrs, ref double MaxX, ref double MaxY, ref double MinX, ref double MinY)
		{
			foreach (Vec2d[] varr2 in arrs)
			{
				int i = 0;
				fixed (Vec2d* varr3 = varr2)
				{
					Vec2d* varr = varr3;
					while (i < varr2.Length)
					{
						if (varr->X > MaxX)
							MaxX = varr->X;
						else if (varr->X < MinX)
							MinX = varr->X;
						if (varr->Y > MaxY)
							MaxY = varr->Y;
						else if (varr->Y < MinY)
							MinY = varr->Y;
						varr++;
						i++;
					}
				}
			}
		}

		#region Point In Polygon
		// On a 0.5 GFlop machine, this produces a 0.5 second
		// decrease in rendering time for the string "Hello!"
		// at 600 point font vs. safe code. Removing index lookups
		// reduces the time at 100 point by 0.010 seconds.
		private static unsafe bool PointInPolygon(Vec2d[] pPts, Vec2d point)
		{
			int id;
			bool c = false;
			fixed (Vec2d* pts = pPts)
			{
				double py = point.Y;
				double px = point.X;
				int len = pPts.Length;
				Vec2d* i = pts;
				Vec2d* j = &pts[len - 1];
				for (id = 0; id < len; j = i++, id++)
				{
					if ((((i->Y <= py) && (py < j->Y)) || ((j->Y <= py) && (py < i->Y))) && (px < (j->X - i->X) * (py - i->Y) / (j->Y - i->Y) + i->X))
						c = !c;
				}
			}
			return c;
		}
		#endregion

		#endregion
	}
}
