#define AllowInvalidProjAndFreeVectors

using System;
using System.Collections.Generic;
using System.Text;
using Orvid.Config;

namespace Orvid.TrueType
{
	public delegate void HintingMethod(GraphicsState state);

	public class GraphicsState
	{
		/// <summary>
		/// The version number to report when a
		/// GetInfo instruction is executed.
		/// </summary>
		/// <remarks>
		/// We currently use 38, which corresponds
		/// to MS Rasterizer v1.9
		/// </remarks>
		private const byte TrueTypeEngineVersion = 38;

		public double UnitsToPixel(double val)
        {
			return val * UnitsToPixelRatio;
		}

		/// <remarks>
		/// We have this in addition to the
		/// one above because integer math
		/// is much faster than floating point
		/// math, and all of the above was floating
		/// point math, vs. only the division here.
		/// </remarks>
		public double UnitsToPixel(int val)
		{
			return val * UnitsToPixelRatio;
		}

		private double UnitsToPixelRatio = 0.0d;

		public double GetPixelsPerEM()
		{
			return ((SizeInPoints * FontSettings.HorizontalDPI) / 72);
		}

		public int GetInfo(int selector)
		{
			const uint VersionMask = 0x0001;
			const uint RotationMask = 0x0002;
			const uint StretchMask = 0x0004;
			const uint GreyscaleMask = 0x0020;
			const uint ClearType_EnabledMask = 0x0040;
			const uint ClearType_CompatibleWidthMask = 0x0080;
			const uint ClearType_SymetricalSmoothingMask = 0x0100;
			const uint ClearType_BGR = 0x0200;
			int result = 0;

			if ((selector & VersionMask) != 0)
				result |= (TrueTypeEngineVersion & 0x7F);
			if ((selector & RotationMask) != 0)
				result |= 0; // We don't do any rotation atm.
			if ((selector & StretchMask) != 0)
				result |= 0; // We don't do any stretching atm.
			if ((selector & GreyscaleMask) != 0)
				result |= 0; // We don't do greyscale rendering atm.
			if ((selector & ClearType_EnabledMask) != 0)
				result |= 0; // We don't do ClearType-like rendering yet.
			if ((selector & ClearType_CompatibleWidthMask) != 0)
				result |= 0; // As I have no idea what this is, we certainly don't do it.
			if ((selector & ClearType_SymetricalSmoothingMask) != 0)
				result |= 0; // We don't do any smoothing.
			if ((selector & ClearType_BGR) != 0)
				result |= 0; // We don't have anything like ClearType atm, so this makes no difference.

			return result;
		}

		public ushort UnitsPerEm;
		public uint SizeInPoints;

		public int[] Storage;
		public F26Dot6[] Cvt;

		public int ScanControl = 2;

		public PointZone Zone1;
		public PointZone TwighlightZone;

		public int ProjectionFreedomDotProduct;
		public F2Dot14 Scale_X;
		public F2Dot14 Scale_Y;

		public int InstructControlFlags;
		public VecF2Dot14 Projection_Vector;
		public VecF2Dot14 Freedom_Vector;
		public VecF2Dot14 Dual_Projection_Vector;
		public F26Dot6 Minimum_Distance;
		public F26Dot6 Control_Value_Cut_In;
		public F26Dot6 Single_Width_Cut_In;
		public F26Dot6 Single_Width_Value;
		public RoundingMode RoundMode;
		public int rp0;
		public int rp1;
		public int rp2;
		public int gep0;
		public int gep1;
		public int gep2;
		public PointZone zp0;
		public PointZone zp1;
		public PointZone zp2;
		public int Delta_Base;
		public int Delta_Shift;
		public int Loop;
		public bool Instruct_Control;
		public bool Auto_Flip;
		public bool Scan_Control;

		public LinkedStack<int> Stack = new LinkedStack<int>();
		public Dictionary<int, HintingMethod> Functions = new Dictionary<int, HintingMethod>();

		public F26Dot6 SRound_Period = F26Dot6.Zero;
		public F26Dot6 SRound_Phase = F26Dot6.Zero;
		public F26Dot6 SRound_Threshold = F26Dot6.Zero;
		private static readonly F26Dot6 GridPeriod = F26Dot6.One;

		public void SetSuperRoundMode(int selector)
		{
			switch (selector & 0xC0)
			{
				case 0x00:
					SRound_Period = GridPeriod >> 1;
					break;
				case 0x40:
					SRound_Period = GridPeriod;
					break;
				case 0x80:
					SRound_Period = GridPeriod << 1;
					break;
				case 0xC0:
					SRound_Period = GridPeriod;
					break;
			}

			switch (selector & 0x30)
			{
				case 0x00:
					SRound_Phase = 0;
					break;
				case 0x10:
					SRound_Phase = SRound_Period >> 2;
					break;
				case 0x20:
					SRound_Phase = SRound_Period >> 1;
					break;
				case 0x30:
					SRound_Phase = (SRound_Period * 3) >> 2;
					break;
			}

			if ((selector & 0x0F) == 0)
				SRound_Threshold = SRound_Period - 1;
			else
			{
				SRound_Threshold = (((selector & 0x0F) - 4) * SRound_Period) >> 3;
			}
		}

		/// <summary>
		/// Gets the value of the specified entry
		/// in the Cvt table.
		/// </summary>
		/// <param name="entryNumber">The entry number to retrieve.</param>
		/// <returns>The retrieved value.</returns>
		public F26Dot6 GetCvtEntry(int entryNumber)
		{
			if (entryNumber == -1)
				return 0;
            if (entryNumber >= Cvt.Length)
                return 0;
			return Cvt[entryNumber];
		}

		/// <summary>
		/// Write a value to the cvt table.
		/// </summary>
		/// <param name="LiteralVal">The value to write.</param>
		/// <param name="entryNumber">The entry to write to.</param>
		public static void WriteCvtEntry(int entryNumber, F26Dot6 value, GraphicsState gState)
		{
			if (entryNumber >= gState.Cvt.Length)
				return;
			gState.Cvt[entryNumber] = value;
		}

		/// <summary>
		/// Write a value in FUnits to the cvt table.
		/// </summary>
		/// <param name="val">The value to write.</param>
		/// <param name="entryNumber">The entry to write to.</param>
		public static void WriteCvtEntryF(int val, int entryNumber, GraphicsState gState)
		{
			if (entryNumber >= gState.Cvt.Length)
				return;
			gState.Cvt[entryNumber] = F26Dot6.FromDouble(gState.UnitsToPixel(val));
		}

		/// <summary>
		/// Adds a single Cvt exception entry.
		/// </summary>
		/// <param name="cvtEntry">The entry number.</param>
		/// <param name="descriptor">The descriptor for the value.</param>
		/// <param name="maginitude">The magnitude of the exception.</param>
		public static void AddCvtException(int cvtEntry, int descriptor, int magnitude, GraphicsState gState)
		{
			int ppem = ((descriptor & 0xF0) >> 4) + (magnitude << 4) + gState.Delta_Base;
			if (ppem == (uint)gState.GetPixelsPerEM())
			{
				descriptor = (descriptor & 0x0F) - 8;
				if (descriptor >= 0)
					descriptor++;
				descriptor = (descriptor << 6) >> gState.Delta_Shift;
				gState.Cvt[cvtEntry] += F26Dot6.FromLiteral(descriptor);
			}
		}

		/// <summary>
		/// Adds a set of Cvt exceptions.
		/// </summary>
		public void AddCvtExceptions(int magnitude)
		{
			int count = Stack.Pop();
			for (uint i = 0; i < count; i++)
			{
				int b = Stack.Pop();
				int cvtEntry = Stack.Pop();
				AddCvtException(cvtEntry, b, magnitude, this);
			}
		}

		/// <summary>
		/// Adds a single point exception entry.
		/// </summary>
		/// <param name="pNum">The point number.</param>
		/// <param name="descriptor">The descriptor for the value.</param>
		/// <param name="maginitude">The magnitude of the exception.</param>
		public static void AddPointException(int pNum, int descriptor, int magnitude, GraphicsState gState)
		{
			if (pNum < gState.zp0.PointCount)
			{
				int ppem = ((descriptor & 0xF0) >> 4) + (magnitude << 4) + gState.Delta_Base;
				if (ppem == (uint)gState.GetPixelsPerEM())
				{
					descriptor = (descriptor & 0x0F) - 8;
					if (descriptor >= 0)
						descriptor++;
					descriptor = (descriptor << 6) >> gState.Delta_Shift;
					gState.Move(gState.zp0, pNum, F26Dot6.FromLiteral(descriptor));
				}
			}
			else
			{
				// Otherwise we fail silently,
				// to avoid issues with certain
				// fonts.
			}
		}

		/// <summary>
		/// Adds a set of Point exceptions.
		/// </summary>
		public void AddPointExceptions(int magnitude)
		{
			int count = Stack.Pop();
			for (uint i = 0; i < count; i++)
			{
				int b = Stack.Pop();
				int pointNumber = Stack.Pop();
				AddPointException(pointNumber, b, magnitude, this);
			}
		}

		/// <summary>
		/// Clears the values in the arrays
		/// which represent a specific glyph.
		/// </summary>
		public void ClearZones()
		{
			Array.Clear(Zone1.OriginalContour, 0, Zone1.OriginalContour.Length);
			Array.Clear(Zone1.Contour, 0, Zone1.Contour.Length);
			Array.Clear(Zone1.OnCurve, 0, Zone1.OnCurve.Length);
			Array.Clear(Zone1.EndPointsOfContours, 0, Zone1.EndPointsOfContours.Length);
			Array.Clear(Zone1.TouchedX, 0, Zone1.TouchedX.Length);
			Array.Clear(Zone1.TouchedY, 0, Zone1.TouchedY.Length);
		}

		public void SetDefaultsForGlyphs()
		{
			Projection_Vector = VecF2Dot14.Axis_X;
			Freedom_Vector = VecF2Dot14.Axis_X;
			Dual_Projection_Vector = VecF2Dot14.Axis_X;
			RoundMode = RoundingMode.Grid;
			gep0 = 1;
			gep1 = 1;
			gep2 = 1;
			zp0 = Zone1;
			zp1 = Zone1;
			zp2 = Zone1;
			Loop = 1;
			RecalcProjFreedomDotProduct();
		}

		/// <summary>
		/// Sets the defaults for all
		/// of the graphics state variables.
		/// </summary>
		public void SetDefaults()
		{
			Minimum_Distance = F26Dot6.One;
			Control_Value_Cut_In = 17d / 16d;
			Single_Width_Cut_In = F26Dot6.Zero;
			Single_Width_Value = F26Dot6.Zero;
			InstructControlFlags = 0;
			Delta_Base = 9;
			Delta_Shift = 3;
			rp0 = 0;
			rp1 = 0;
			rp2 = 0;
			Auto_Flip = true;
			Instruct_Control = false;
			Scan_Control = false;
			SetDefaultsForGlyphs();
		}

		public void CalculateScale()
		{
			UnitsToPixelRatio = (double)(SizeInPoints * FontSettings.HorizontalDPI) / (double)(UnitsPerEm * 72);
			double pixPerUnit = UnitsToPixel(1);
			Scale_X = F2Dot14.FromDouble(pixPerUnit);
			Scale_Y = F2Dot14.FromDouble(pixPerUnit);
		}

		#region Shift
		private static void ComputePointDisplacement(ref F26Dot6 dX, ref F26Dot6 dY, bool useZp0, ref PointZone zp, ref int refPoint, GraphicsState gState)
		{
			F26Dot6 projDist;
			if (useZp0)
			{
				zp = gState.zp0;
				refPoint = gState.rp1;
			}
			else
			{
				zp = gState.zp1;
				refPoint = gState.rp2;
			}
			projDist = gState.Project(zp.Contour[refPoint], zp.OriginalContour[refPoint]);

			dX = F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(projDist), F2Dot14.AsLiteral(gState.Freedom_Vector.X) << 16, gState.ProjectionFreedomDotProduct));
			dY = F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(projDist), F2Dot14.AsLiteral(gState.Freedom_Vector.Y) << 16, gState.ProjectionFreedomDotProduct));
		}

		private static void Move_Zp2_Point(int pNum, F26Dot6 dX, F26Dot6 dY, bool touch, GraphicsState gState)
		{
			if (gState.Freedom_Vector.X != 0)
				gState.zp2.Contour[pNum].X += dX;
			if (gState.Freedom_Vector.Y != 0)
				gState.zp2.Contour[pNum].Y += dY;

			if (touch)
				gState.TouchPoint(pNum, gState.zp2, gState.Freedom_Vector);
		}

		/// <summary>
		/// Shifts a contour in accordance with the SHC[a] instruction.
		/// </summary>
		public static void ShiftContour(int contourIndex, bool useZp0, GraphicsState gState)
		{
			int refP = 0, fPoint, lPoint;
			F26Dot6 dX = 0, dY = 0;
			PointZone zp = null;
			ComputePointDisplacement(ref dX, ref dY, useZp0, ref zp, ref refP, gState);

			if (contourIndex == 0)
				fPoint = 0;
			else
				fPoint = gState.Zone1.EndPointsOfContours[contourIndex - 1] + 1;
			lPoint = gState.Zone1.EndPointsOfContours[contourIndex];
			if (gState.zp2 == gState.TwighlightZone)
			{
				throw new Exception("This isn't currently supported!");
			}

			if (zp == gState.zp2)
			{
				for (int i = fPoint; i <= lPoint; i++)
				{
					if (refP != i)
						Move_Zp2_Point(i, dX, dY, true, gState);
				}
			}
			else
			{
				for (int i = fPoint; i <= lPoint; i++)
				{
					Move_Zp2_Point(i, dX, dY, true, gState);
				}
			}
		}

		/// <summary>
		/// Shifts a set of points by a specified
		/// ammount, in accordance with the ShPix[]
		/// instruction.
		/// </summary>
		public static void LoopedShiftPixel(F26Dot6 dist, GraphicsState gState)
		{
			F26Dot6 dX = dist * gState.Freedom_Vector.X;
			F26Dot6 dY = dist * gState.Freedom_Vector.Y;
			for (int i = 0; i < gState.Loop; i++)
			{
				Move_Zp2_Point(gState.Stack.Pop(), dX, dY, true, gState);
			}
			gState.Loop = 1;
		}

		/// <summary>
		/// Shifts a set of points in accordance 
		/// with the ShP[] instruction.
		/// </summary>
		public static void LoopedShiftPoint(bool useZp0, GraphicsState gState)
		{
			F26Dot6 dX = 0, dY = 0;
			int refP = 0;
			PointZone zp = null;
			ComputePointDisplacement(ref dX, ref dY, useZp0, ref zp, ref refP, gState);
			for (int i = 0; i < gState.Loop; i++)
			{
				Move_Zp2_Point(gState.Stack.Pop(), dX, dY, true, gState);
			}
			gState.Loop = 1;
		}

		#endregion

		#region Project
		/// <summary>
		/// Projects the specified coordinate onto
		/// the projection vector, and returns how
		/// far from the origin it is.
		/// </summary>
		private F26Dot6 Project(VecF26Dot6 coord)
		{
			return DotProduct(coord.X, coord.Y, Projection_Vector.X, Projection_Vector.Y);
		}

		/// <summary>
		/// Projects the specified coordinate onto
		/// the dual projection vector, and returns 
		/// how far from the origin it is.
		/// </summary>
		private F26Dot6 DualProject(VecF26Dot6 coord)
		{
			return DotProduct(coord.X, coord.Y, Dual_Projection_Vector.X, Dual_Projection_Vector.Y);
		}

		private F26Dot6 Project(VecF26Dot6 coordA, VecF26Dot6 coordB)
		{
			return Project(new VecF26Dot6(coordA.X - coordB.X, coordA.Y - coordB.Y));
		}
		private F26Dot6 DualProject(VecF26Dot6 coordA, VecF26Dot6 coordB)
		{
			return DualProject(new VecF26Dot6(coordA.X - coordB.X, coordA.Y - coordB.Y));
		}
		#endregion


		#region Math

		/// <summary>
		/// Computes (a * b) / c with maximum
		/// accuracy.
		/// </summary>
		private static int MulDiv(int a, int b, int c)
		{
			int s;
			long d;

			s = 1;
			if (a < 0) { a = -a; s = -s; }
			if (b < 0) { b = -b; s = -s; }
			if (c < 0) { c = -c; s = -s; }

			d = (long)(c > 0 ? ((long)a * b + (c >> 1)) / c : 0x7FFFFFFFL);

			return (int)((s > 0) ? d : -d);
		}

		/// <summary>
		/// Computes a dot product.
		/// </summary>
		private static F26Dot6 DotProduct(F26Dot6 pAx, F26Dot6 pAy, F2Dot14 pBx, F2Dot14 pBy)
		{
			int m, s, hi1, hi2, hi;
			uint l, lo1, lo2, lo;

			int ax = F26Dot6.AsLiteral(pAx);
			int ay = F26Dot6.AsLiteral(pAy);
			int bx = F2Dot14.AsLiteral(pBx);
			int by = F2Dot14.AsLiteral(pBy);

			l = (uint)((ax & 0xFFFFU) * bx);
			m = (ax >> 16) * bx;
			lo1 = l + (uint)(m << 16);

			hi1 = (m >> 16) + ((int)l >> 31) + (lo1 < l ? 1 : 0);
			l = (uint)((ay & 0xFFFFU) * by);
			m = (ay >> 16) * by;
			lo2 = l + (uint)(m << 16);
			hi2 = (m >> 16) + ((int)l >> 31) + (lo2 < l ? 1 : 0);
			lo = lo1 + lo2;
			hi = hi1 + hi2 + (lo < lo1 ? 1 : 0);
			s = hi >> 31;
			l = lo + (uint)s;
			hi += s + (l < lo ? 1 : 0);
			lo = l;
			l = lo + 0x2000U;
			hi += (l < lo ? 1 : 0);

			return F26Dot6.FromLiteral((int)((uint)(hi << 18) | (uint)(l >> 14)));
		}

		#region Code for Vector Length (Yikes....)
		private static int VectorLength(VecF26Dot6 vec)
		{
			int shift;
			VecF26Dot6 v;
			v = vec;
			if (v.X == 0)
			{
				return F26Dot6.AsLiteral((v.Y >= 0) ? v.Y : -v.Y);
			}
			else if (v.Y == 0)
			{
				return F26Dot6.AsLiteral((v.X >= 0) ? v.X : -v.X);
			}

			/* general case */
			shift = Prenorm(ref v);
			PseudoPolarize(ref v);
			v.X = F26Dot6.FromLiteral(Downscale(F26Dot6.AsLiteral(v.X)));
			if (shift > 0)
				return F26Dot6.AsLiteral((v.X + (1 << (shift - 1))) >> shift);

			return F26Dot6.AsLiteral(v.X << -shift);
		}

		private static int Downscale(int val)
		{
			int s;
			Int64 v;
			s = val;
			val = (val >= 0) ? val : -val;
			v = (val * 1166391785) + 0x100000000;
			val = (int)(v >> 32);
			return (s >= 0) ? val : -val;
		}

		private static readonly int[] ArcTanTabl = new int[]  
		{
			4157273, 2949120, 1740967, 919879, 466945, 234379, 117304,
			58666, 29335, 14668, 7334, 3667, 1833, 917, 458, 229, 115,
			57, 29, 14, 7, 4, 2, 1
		};
		private static unsafe void PseudoPolarize(ref VecF26Dot6 vec)
		{
			int theta;
			int yi, i;
			int x, y;
			int* arctanptr;
			fixed (int* arctanptrLock = ArcTanTabl)
			{
				arctanptr = arctanptrLock;
				x = F26Dot6.AsLiteral(vec.X);
				y = F26Dot6.AsLiteral(vec.Y);
				theta = 0;
				if (x < 0)
				{
					x = -x;
					y = -y;
					theta = 180 << 17;
				}
				if (y > 0)
					theta = -theta;
				if (y < 0)
				{
					yi = y + (x << 1);
					x = x - (y << 1);
					y = yi;
					theta -= *arctanptr++;
				}
				else
				{
					yi = y - (x << 1);
					x = x + (y << 1);
					y = yi;
					theta += *arctanptr++;
				}

				i = 0;
				do
				{
					if (y < 0)
					{
						yi = y + (x >> i);
						x = x - (y >> i);
						y = yi;
						theta -= *arctanptr++;
					}
					else
					{
						yi = y - (x >> i);
						x = x + (y >> i);
						y = yi;
						theta += *arctanptr++;
					}
				} while (++i < 23);
				if (theta >= 0)
					theta = PadRound(theta, 32);
				else
					theta = -PadRound(-theta, 32);

				vec.X = F26Dot6.FromLiteral(x);
				vec.Y = F26Dot6.FromLiteral(theta);
			}
		}

		private static int PadRound(int x, int n)
		{
			return (int)(((x) + ((n) / 2)) & ~((n) - 1));
		}
		private static int Prenorm(ref VecF26Dot6 vec)
		{
			int x, y, z, shift;


			x = F26Dot6.AsLiteral(vec.X);
			y = F26Dot6.AsLiteral(vec.Y);

			z = ((x >= 0) ? x : -x) | ((y >= 0) ? y : -y);
			shift = 0;

			if (z >= (1L << 16))
			{
				z >>= 16;
				shift += 16;
			}
			if (z >= (1L << 8))
			{
				z >>= 8;
				shift += 8;
			}
			if (z >= (1L << 4))
			{
				z >>= 4;
				shift += 4;
			}
			if (z >= (1L << 2))
			{
				z >>= 2;
				shift += 2;
			}
			if (z >= (1L << 1))
			{
				z >>= 1;
				shift += 1;
			}

			if (shift <= 27)
			{
				shift = 27 - shift;
				vec.X = F26Dot6.FromLiteral(x << shift);
				vec.Y = F26Dot6.FromLiteral(y << shift);
			}
			else
			{
				shift -= 27;
				vec.X = F26Dot6.FromLiteral(x >> shift);
				vec.Y = F26Dot6.FromLiteral(y >> shift);
				shift = -shift;
			}
			return shift;
		}

		#endregion

		#endregion


		#region Move
		/// <summary>
		/// Moves a point to a specified location.
		/// </summary>
		private void Move(PointZone zone, int pointNumber, F26Dot6 distance)
		{
			F2Dot14 vec = Freedom_Vector.X;
			if (vec != F2Dot14.Zero)
			{
				zone.Contour[pointNumber].X += F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(distance),
													  F2Dot14.AsLiteral(vec) << 16,
													  ProjectionFreedomDotProduct));
			}
			vec = Freedom_Vector.Y;
			if (vec != F2Dot14.Zero)
			{
				zone.Contour[pointNumber].Y += F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(distance),
													  F2Dot14.AsLiteral(vec) << 16,
													  ProjectionFreedomDotProduct));
			}
			TouchPoint(pointNumber, zone, Freedom_Vector);
		}

		/// <summary>
		/// Moves the original location of a point to
		/// a specified location. Doesn't touch the point.
		/// </summary>
		private void MoveOriginal(PointZone zone, int pointNumber, F26Dot6 distance)
		{
			F2Dot14 vec = Freedom_Vector.X;
			if (vec != F2Dot14.Zero)
			{
				zone.Contour[pointNumber].X += MulDiv(F26Dot6.AsLiteral(distance),
													  F2Dot14.AsLiteral(vec) << 16,
													  ProjectionFreedomDotProduct);
			}
			vec = Freedom_Vector.Y;
			if (vec != F2Dot14.Zero)
			{
				zone.Contour[pointNumber].Y += MulDiv(F26Dot6.AsLiteral(distance),
													  F2Dot14.AsLiteral(vec) << 16,
													  ProjectionFreedomDotProduct);
			}
		}
		#endregion


		#region Move (Stack) Direct/Indirect Relative/Absolute Point

		/// <summary>
		/// See documentation for MIAP[a]
		/// </summary>
		/// <param name="cvt">The cvt entry number to use.</param>
		/// <param name="pNum">The point number to move.</param>
		/// <param name="useCvtAndRound">True if it should use the cvt cut-in value, and round.</param>
		public static void MoveIndirectAbsolutePoint(int cvt, int pNum, bool useCvtAndRound, GraphicsState gState)
		{
			F26Dot6 distance = gState.Cvt[cvt];

			// Twilight zone
			if (gState.gep0 == 0)
			{
				gState.zp0.OriginalContour[pNum].X = distance * gState.Freedom_Vector.X;
				gState.zp0.OriginalContour[pNum].Y = distance * gState.Freedom_Vector.Y;
				gState.zp0.Contour[pNum] = gState.zp0.OriginalContour[pNum];
			}

			F26Dot6 originalDistance = gState.Project(gState.zp0.Contour[pNum]);
			if (useCvtAndRound)
			{
				if (F26Dot6.Abs(distance - originalDistance) > gState.Control_Value_Cut_In)
					distance = originalDistance;

				distance = gState.Round(distance, DistanceType.Grey);
			}

			gState.Move(gState.zp0, pNum, distance - originalDistance);

			gState.rp0 = pNum;
			gState.rp1 = pNum;
		}

		/// <summary>
		/// See documentation for MDAP[a]
		/// </summary>
		/// <param name="pNum">The point number to move.</param>
		/// <param name="round">True if it needs to round.</param>
		public static void MoveDirectAbsolutePoint(int pNum, bool round, GraphicsState gState)
		{
			F26Dot6 dist;
			if (round)
			{
				F26Dot6 curDist = gState.Project(gState.zp0.Contour[pNum]);
				dist = gState.Round(curDist, DistanceType.Grey) - curDist;
			}
			else
			{
				dist = F26Dot6.Zero;
			}
			gState.Move(gState.zp0, pNum, dist);
			gState.rp0 = pNum;
			gState.rp1 = pNum;
		}

		/// <summary>
		/// See documentation for MIRP[abcde]
		/// </summary>
		public static void MoveIndirectRelativePoint(int cvtEntry, int pNum, bool cvtAndRound, bool minDistance, bool setRp0, DistanceType dType, GraphicsState gState)
		{
			F26Dot6 cvtDist, curDist, origDist, dist;

			cvtDist = gState.GetCvtEntry(cvtEntry);
			if (F26Dot6.Abs(cvtDist - gState.Single_Width_Value) < gState.Single_Width_Cut_In)
			{
				if (cvtDist >= F26Dot6.Zero)
					cvtDist = gState.Single_Width_Value;
				else
					cvtDist = -gState.Single_Width_Value;
			}

			if (gState.gep1 == 0)
			{
				gState.zp1.OriginalContour[pNum].X = gState.zp0.OriginalContour[gState.rp0].X + (cvtDist * gState.Freedom_Vector.X);
				gState.zp1.OriginalContour[pNum].Y = gState.zp0.OriginalContour[gState.rp0].Y + (cvtDist * gState.Freedom_Vector.Y);
				gState.zp1.Contour[pNum] = gState.zp0.Contour[pNum];
			}

			origDist = gState.DualProject(gState.zp1.OriginalContour[pNum], gState.zp0.OriginalContour[gState.rp0]);
			curDist = gState.Project(gState.zp1.Contour[pNum], gState.zp0.Contour[gState.rp0]);

			if (gState.Auto_Flip)
			{
				if ((origDist ^ cvtDist) < F26Dot6.Zero)
					cvtDist = -cvtDist;
			}

			if (cvtAndRound)
			{
				if (gState.gep0 == gState.gep1)
				{
					if (F26Dot6.Abs(cvtDist - origDist) > gState.Control_Value_Cut_In)
						cvtDist = origDist;
				}
				dist = gState.Round(cvtDist, dType);
			}
			else
			{
				dist = Round_None(cvtDist);
			}

			if (minDistance)
			{
				if (origDist >= F26Dot6.Zero)
				{
					if (dist < gState.Minimum_Distance)
						dist = gState.Minimum_Distance;
				}
				else
				{
					if (dist > -gState.Minimum_Distance)
						dist = -gState.Minimum_Distance;
				}
			}

			gState.Move(gState.zp1, pNum, dist - curDist);
			gState.rp1 = gState.rp0;
			if (setRp0)
				gState.rp0 = pNum;
			gState.rp2 = pNum;
		}

		/// <summary>
		/// See documentation for MDRP[abcde]
		/// </summary>
		public static void MoveDirectRelativePoint(int pNum, bool Round, bool minDistance, bool setRp0, DistanceType dType, GraphicsState gState)
		{
			F26Dot6 origDist, dist;

			if (gState.gep0 == 0 || gState.gep1 == 0)
			{
				origDist = gState.DualProject(gState.zp1.OriginalContour[pNum], gState.zp0.OriginalContour[gState.rp0]);
			}
			else
			{
				VecF26Dot6 v1 = gState.zp1.OriginalUnscaledContour[pNum];
				VecF26Dot6 v2 = gState.zp0.OriginalUnscaledContour[gState.rp0];
				if (gState.Scale_X == gState.Scale_Y)
				{
					origDist = gState.DualProject(v1, v2);
					origDist = origDist * gState.Scale_X;
				}
				else
				{
					VecF26Dot6 fV = VecF26Dot6.Zero;

					fV.X = (v1.X - v2.X) * gState.Scale_X;
					fV.Y = (v1.Y - v2.Y) * gState.Scale_Y;
					origDist = gState.DualProject(fV);
				}
			}

			if (F26Dot6.Abs(origDist - gState.Single_Width_Value) < gState.Single_Width_Cut_In)
			{
				if (origDist >= F26Dot6.Zero)
					origDist = gState.Single_Width_Value;
				else
					origDist = -gState.Single_Width_Value;
			}

			if (Round)
			{
				dist = gState.Round(origDist, dType);
			}
			else
			{
				dist = Round_None(origDist);
			}

			if (minDistance)
			{
				if (origDist >= F26Dot6.Zero)
				{
					if (dist < gState.Minimum_Distance)
						dist = gState.Minimum_Distance;
				}
				else
				{
					if (dist > -gState.Minimum_Distance)
						dist = -gState.Minimum_Distance;
				}
			}
			origDist = gState.Project(gState.zp1.Contour[pNum], gState.zp0.Contour[gState.rp0]);
			gState.Move(gState.zp1, pNum, dist - origDist);


			gState.rp1 = gState.rp0;
			gState.rp2 = pNum;
			if (setRp0)
				gState.rp0 = pNum;
		}

		/// <summary>
		/// See documentation for MSIRP[a]
		/// </summary>
		/// <param name="distance">The distance to move it to.</param>
		/// <param name="pNum">The point number to move.</param>
		public static void MoveStackIndirectRelativePoint(F26Dot6 distance, int pNum, bool setRp0, GraphicsState gState)
		{
			if (gState.gep1 == 0)
			{
				gState.zp1.OriginalContour[pNum] = gState.zp0.OriginalContour[gState.rp0];
				gState.MoveOriginal(gState.zp1, pNum, distance);
				gState.zp1.Contour[pNum] = gState.zp1.OriginalContour[pNum];
			}
			F26Dot6 curDistance = gState.Project(gState.zp1.Contour[pNum], gState.zp0.Contour[gState.rp0]);
			gState.Move(gState.zp1, pNum, distance - curDistance);
			gState.rp1 = gState.rp0;
			gState.rp2 = pNum;
			if (setRp0)
				gState.rp0 = pNum;
		}
		#endregion


		#region TouchPoint
		/// <summary>
		/// Touches the specified point
		/// along the specified axis.
		/// </summary>
		/// <param name="pNum">The point number to touch.</param>
		/// <param name="pz">The point zone in which to touch.</param>
		/// <param name="tAxis">The axis to touch along.</param>
		public void TouchPoint(int pNum, PointZone pz, VecF2Dot14 tAxis)
		{
			if (pz == TwighlightZone)
			{
				// Points in the twilight zone
				// don't get touched.
			}
			else
			{
				if (tAxis == VecF2Dot14.Axis_X)
				{
					pz.TouchedX[pNum] = true;
				}
				else if (tAxis == VecF2Dot14.Axis_Y)
				{
					pz.TouchedY[pNum] = true;
				}
				else
				{
					pz.TouchedX[pNum] = true;
					pz.TouchedY[pNum] = true;
				}
			}
		}
		#endregion


		#region Set Projection & Freedom Vectors
		private static bool Normalize(int Vx, int Vy, ref VecF2Dot14 destVec)
		{
			int W;
			bool S1, S2;
			if (F26Dot6.Abs(Vx) < 0x10000L && F26Dot6.Abs(Vy) < 0x10000L)
			{
				Vx <<= 8;
				Vy <<= 8;

				W = VectorLength(new VecF26Dot6(F26Dot6.FromLiteral(Vx), F26Dot6.FromLiteral(Vy)));

				if (W == 0)
				{
					return true;
				}

				destVec.X = F2Dot14.FromLiteral(MulDiv(Vx, 0x4000, W));
				destVec.Y = F2Dot14.FromLiteral(MulDiv(Vy, 0x4000, W));

				return true;
			}

			W = VectorLength(new VecF26Dot6(F26Dot6.FromLiteral(Vx), F26Dot6.FromLiteral(Vy)));

			Vx = MulDiv(Vx, 0x4000, W);
			Vy = MulDiv(Vy, 0x4000, W);

			W = Vx * Vx + Vy * Vy;

			/* Now, we want that Sqrt( W ) = 0x4000 */
			/* Or 0x10000000 <= W < 0x10004000        */

			if (Vx < 0)
			{
				Vx = -Vx;
				S1 = true;
			}
			else
				S1 = false;

			if (Vy < 0)
			{
				Vy = -Vy;
				S2 = true;
			}
			else
				S2 = false;

			while (W < 0x10000000L)
			{
				if (Vx < Vy)
					Vx++;
				else
					Vy++;
				W = Vx * Vx + Vy * Vy;
			}

			while (W >= 0x10004000L)
			{
				if (Vx < Vy)
					Vx--;
				else
					Vy--;
				W = Vx * Vx + Vy * Vy;
			}
			if (S1)
				Vx = -Vx;
			if (S2)
				Vy = -Vy;
			destVec.X = F2Dot14.FromLiteral(Vx);
			destVec.Y = F2Dot14.FromLiteral(Vy);

			return true;
		}

		/// <summary>
		/// Sets the projection vector as specified
		/// by the params.
		/// </summary>
		/// <param name="y">The Y portion of the vector.</param>
		/// <param name="x">The X portion of the vector.</param>
		public void SetProjectionVector(F2Dot14 y, F2Dot14 x)
		{
			Normalize(F2Dot14.AsLiteral(x), F2Dot14.AsLiteral(y), ref Projection_Vector);
			RecalcProjFreedomDotProduct();
			Dual_Projection_Vector = Projection_Vector;
		}

		/// <summary>
		/// Sets the freedom vector to a specified line.
		/// </summary>
		public static void SetFreedomVectorToLine(int pNum2, int pNum1, bool perpindicular, GraphicsState gState)
		{
			SetVectorToLine(gState.zp1.Contour[pNum1], gState.zp2.Contour[pNum2], perpindicular, ref gState.Freedom_Vector);
		}

		/// <summary>
		/// Sets the projection vector to a specified line.
		/// </summary>
		public static void SetProjectionVectorToLine(int pNum2, int pNum1, bool perpindicular, GraphicsState gState)
		{
			SetVectorToLine(gState.zp1.Contour[pNum1], gState.zp2.Contour[pNum2], perpindicular, ref gState.Projection_Vector);
		}

		/// <summary>
		/// Sets the dual projection vector to a specified line.
		/// </summary>
		public static void SetDualProjectionVectorToLine(int pNum2, int pNum1, bool perpindicular, GraphicsState gState)
		{
			SetVectorToLine(gState.zp1.Contour[pNum1], gState.zp2.Contour[pNum2], perpindicular, ref gState.Dual_Projection_Vector);
		}

		/// <summary>
		/// Sets a vector to the line between 2 points.
		/// </summary>
		public static void SetVectorToLine(VecF26Dot6 p1, VecF26Dot6 p2, bool perpindicular, ref VecF2Dot14 dVector)
		{
			F26Dot6 lA = p1.X - p2.X;
			F26Dot6 lB = p1.Y - p2.Y;
			if (lA == 0 && lB == 0)
			{
				lA = F26Dot6.FromLiteral(0x4000);
			}
			else if (perpindicular)
			{
				F26Dot6 lC = lB;
				lB = lA;
				lA = -lC;
			}
			Normalize(F26Dot6.AsLiteral(lA), F26Dot6.AsLiteral(lB), ref dVector);
		}

		/// <summary>
		/// Sets the freedom vector as specified
		/// by the params.
		/// </summary>
		/// <param name="y">The Y portion of the vector.</param>
		/// <param name="x">The X portion of the vector.</param>
		public void SetFreedomVector(F2Dot14 y, F2Dot14 x)
		{
			Normalize(F2Dot14.AsLiteral(x), F2Dot14.AsLiteral(y), ref Freedom_Vector);
			RecalcProjFreedomDotProduct();
		}

		/// <summary>
		/// Recalculates the ProjectionFreedomDotProduct.
		/// </summary>
		public void RecalcProjFreedomDotProduct()
		{
			ProjectionFreedomDotProduct =
				((F2Dot14.AsLiteral(Projection_Vector.X) * F2Dot14.AsLiteral(Freedom_Vector.X)) << 2)
					+
				((F2Dot14.AsLiteral(Projection_Vector.Y) * F2Dot14.AsLiteral(Freedom_Vector.Y)) << 2)
			;
		}
		#endregion


		#region Interpolate Points

		#region InterpolatePoints
		/// <summary>
		/// Interpolates a set of points in accordance with
		/// the IP[] instruction.
		/// </summary>
		public static void InterpolatePoints(GraphicsState gState)
		{
			bool twilight = gState.gep0 == 0 || gState.gep1 == 0 || gState.gep2 == 0;
			VecF26Dot6 orusBase, curBase;
			F26Dot6 oldRange, curRange;

			if (twilight)
				orusBase = gState.zp0.OriginalContour[gState.rp1];
			else
				orusBase = gState.zp0.OriginalUnscaledContour[gState.rp1];
			curBase = gState.zp0.Contour[gState.rp1];

			if (gState.rp1 >= gState.zp0.PointCount || gState.rp2 >= gState.zp1.PointCount)
			{
				oldRange = 0;
				curRange = 0;
				Console.WriteLine("WARNING: InterpolatePoints: either rp1 or rp2 was out of bounds!");
			}
			else
			{
				if (twilight)
					oldRange = gState.DualProject(gState.zp1.OriginalContour[gState.rp2], orusBase);
				else if (gState.Scale_X == gState.Scale_Y)
					oldRange = gState.DualProject(gState.zp1.OriginalUnscaledContour[gState.rp2], orusBase);
				else
					throw new Exception("Non-uniform scale not currently supported!");

				curRange = gState.Project(gState.zp1.Contour[gState.rp2], curBase);
			}

			for (uint i = 0; i < gState.Loop; i++)
			{
				int pNum = gState.Stack.Pop();
				F26Dot6 origDist, curDist, newDist;

				if (twilight)
					origDist = gState.DualProject(gState.zp2.OriginalContour[pNum], orusBase);
				else if (gState.Scale_X == gState.Scale_Y)
					origDist = gState.DualProject(gState.zp2.OriginalUnscaledContour[pNum], orusBase);
				else
					throw new Exception("Non-uniform scale not currently supported!");

				curDist = gState.Project(gState.zp2.Contour[pNum], curBase);
				if (origDist != 0)
					newDist = (oldRange != 0) ? F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(origDist), F26Dot6.AsLiteral(curRange), F26Dot6.AsLiteral(oldRange))) : curDist;
				else
					newDist = 0;

				gState.Move(gState.zp2, pNum, newDist - curDist);
			}
			gState.Loop = 1;
		}
		#endregion

		#region Interpolate Untouched Points
		private struct IUP_Worker
		{
			public F26Dot6[] CurrentPositions;
			public F26Dot6[] OriginalPositions;
			public F26Dot6[] OriginalUnscaledPositions;
			public bool IsAxisX;
			public bool[] TouchedArray;

			#region Interpolate
			public unsafe void Interpolate(int p1, int p2, int ref1, int ref2, GraphicsState gState)
			{
				if (p1 > p2)
					return;

				if (ref1 >= this.CurrentPositions.Length || ref2 >= this.CurrentPositions.Length)
					return;

				F26Dot6 orus1, orus2, orig1, orig2, delta1, delta2;

				orus1 = OriginalUnscaledPositions[ref1];
				orus2 = OriginalUnscaledPositions[ref2];

				if (orus1 > orus2)
				{
					F26Dot6 tmpO = orus1;
					orus1 = orus2;
					orus2 = tmpO;

					int tmpR = ref1;
					ref1 = ref2;
					ref2 = tmpR;
				}

				orig1 = OriginalPositions[ref1];
				orig2 = OriginalPositions[ref2];
				delta1 = CurrentPositions[ref1] - orig1;
				delta2 = CurrentPositions[ref2] - orig2;

				if (orus1 == orus2)
				{
					fixed (F26Dot6* oPos2 = OriginalPositions)
					{
						fixed (F26Dot6* cPos2 = CurrentPositions)
						{
							int* oPos = (int*)(void*)&oPos2[p1];
							int* cPos = (int*)(void*)&cPos2[p1];
							int len = p2 - p1;
							int i = 0;
							int iDelta1 = *((int*)(void*)&delta1);
							int iDelta2 = *((int*)(void*)&delta2);
							int iOrig2 = *((int*)(void*)&orig1);
							while (i <= len)
							{
								*cPos = *oPos + ((*oPos <= iOrig2) ? iDelta1 : iDelta2);
								oPos++;
								cPos++;
								i++;
							}
						}
					}
				}
				else
				{
					for (int i = p1; i <= p2; i++)
					{
						F26Dot6 pos = OriginalPositions[i];

						if (pos <= orig1)
						{
							pos += delta1;
						}
						else if (pos >= orig2)
						{
							pos += delta2;
						}
						else
						{
							F26Dot6 dA = pos - orig1;
							F26Dot6 dB = orig2 - pos;
							F26Dot6 tot = (orig2 - orig1);
							if (tot < 0)
							{
								throw new Exception("This shouldn't happen!");
							}

							//F26Dot6 dAFRatio = dA / tot;
							//F26Dot6 dBFRatio = dB / tot;
							//F26Dot6 dFRatTot = dAFRatio + dBFRatio;
							//F26Dot6 tFDelta = (dAFRatio * (delta1)) + (dBFRatio * (delta2));

							double dTot = F26Dot6.ToDouble(tot);
							double dADeltaRatio = F26Dot6.ToDouble(dA) / dTot;
							double dBDeltaRatio = F26Dot6.ToDouble(dB) / dTot;
							double dRatTot = dADeltaRatio + dBDeltaRatio;
							if (dRatTot != 1.0d)
							{
								throw new Exception("Well, not always a bad thing, but still may be.");
							}
							double tDelta = (dADeltaRatio * F26Dot6.ToDouble(delta1)) + (dBDeltaRatio * F26Dot6.ToDouble(delta2));
							F26Dot6 fDelta = F26Dot6.FromDouble(tDelta);

							pos += fDelta;
							
						}

						CurrentPositions[i] = pos;
					}
				}
			}
			#endregion

			#region Shift
			public unsafe void Shift(int p1, int p2, int p)
			{
				F26Dot6 delta = CurrentPositions[p] - OriginalPositions[p];
				if (delta != 0)
				{
					fixed (F26Dot6* cPos2 = CurrentPositions)
					{
						int* pos = (int*)(void*)&cPos2[p1];
						int iDelta = *((int*)(void*)&delta);
						int len = p - p1;
						int i = 0;
						// Everything from here on is
						// done in registers, even on
						// a system with only 4 registers.
						while (i < len)
						{
							*pos += iDelta;
							i++;
							pos++;
						}
						pos++;
						i++;
						while (i <= p2)
						{
							*pos += iDelta;
							i++;
							pos++;
						}
					}
				}
			}
			#endregion
		}

		#region Unsafe Copy

		#region Copy In
		private static unsafe void UnsafeCopyInX(F26Dot6[] pDest, VecF26Dot6[] pSrc)
		{
			fixed (F26Dot6* dest2 = pDest)
			{
				fixed (VecF26Dot6* src2 = pSrc)
				{
					F26Dot6* dest = dest2;
					VecF26Dot6* src = src2;
					int len = pDest.Length;
					int i = 0;
					while (i < len)
					{
						*dest = src->X;
						src++;
						dest++;
						i++;
					}
				}
			}
		}

		private static unsafe void UnsafeCopyInY(F26Dot6[] pDest, VecF26Dot6[] pSrc)
		{
			fixed (F26Dot6* dest2 = pDest)
			{
				fixed (VecF26Dot6* src2 = pSrc)
				{
					F26Dot6* dest = dest2;
					VecF26Dot6* src = src2;
					int len = pDest.Length;
					int i = 0;
					while (i < len)
					{
						*dest = src->Y;
						src++;
						dest++;
						i++;
					}
				}
			}
		}
		#endregion

		#region Copy Out
		private static unsafe void UnsafeCopyOutX(VecF26Dot6[] pDest, F26Dot6[] pSrc)
		{
			fixed (VecF26Dot6* dest2 = pDest)
			{
				fixed (F26Dot6* src2 = pSrc)
				{
					VecF26Dot6* dest = dest2;
					F26Dot6* src = src2;
					int len = pSrc.Length;
					int i = 0;
					while (i < len)
					{
						dest->X = *src;
						src++;
						dest++;
						i++;
					}
				}
			}
		}

		private static unsafe void UnsafeCopyOutY(VecF26Dot6[] pDest, F26Dot6[] pSrc)
		{
			fixed (VecF26Dot6* dest2 = pDest)
			{
				fixed (F26Dot6* src2 = pSrc)
				{
					VecF26Dot6* dest = dest2;
					F26Dot6* src = src2;
					int len = pSrc.Length;
					int i = 0;
					while (i < len)
					{
						dest->Y = *src;
						src++;
						dest++;
						i++;
					}
				}
			}
		}
		#endregion

		#endregion

		public static unsafe void InterpolateUntouchedPoints(bool pXAxis, GraphicsState gState)
		{
			// Nothing to interpolate.
			if (gState.Zone1.ContourCount == 0)
				return;
			int fPoint, ePoint;
			int fTouched, curTouched;
			int curPoint, curContour;
			IUP_Worker worker = new IUP_Worker();

			#region Initialize Worker
			if (pXAxis)
			{

				int pCount = gState.Zone1.PointCount;
				worker.CurrentPositions = new F26Dot6[pCount];
				UnsafeCopyInX(worker.CurrentPositions, gState.Zone1.Contour);
				worker.OriginalPositions = new F26Dot6[pCount];
				UnsafeCopyInX(worker.OriginalPositions, gState.Zone1.OriginalContour);
				worker.OriginalUnscaledPositions = new F26Dot6[pCount];
				UnsafeCopyInX(worker.OriginalUnscaledPositions, gState.Zone1.OriginalUnscaledContour);
				worker.TouchedArray = gState.Zone1.TouchedX;
				worker.IsAxisX = true;
			}
			else
			{
				int pCount = gState.Zone1.PointCount;
				worker.CurrentPositions = new F26Dot6[pCount];
				UnsafeCopyInY(worker.CurrentPositions, gState.Zone1.Contour);
				worker.OriginalPositions = new F26Dot6[pCount];
				UnsafeCopyInY(worker.OriginalPositions, gState.Zone1.OriginalContour);
				worker.OriginalUnscaledPositions = new F26Dot6[pCount];
				UnsafeCopyInY(worker.OriginalUnscaledPositions, gState.Zone1.OriginalUnscaledContour);
				worker.TouchedArray = gState.Zone1.TouchedY;
				worker.IsAxisX = false;
			}
			#endregion

#if WriteDebug
			System.IO.StreamWriter swrtr = new System.IO.StreamWriter("tmp.txt");
			foreach (F26Dot6 f in worker.CurrentPositions)
			{
				swrtr.WriteLine("Current Position: " + f.ToString());
			}
#endif

			curContour = 0;
			curPoint = 0;
			do
			{
				ePoint = gState.Zone1.EndPointsOfContours[curContour];
				fPoint = curPoint;

#warning We may need a bounds check here due to potential adjustment

				fixed (bool* tarr4 = worker.TouchedArray)
				{
					while (curPoint <= ePoint && !tarr4[curPoint])
						curPoint++;
				}

				if (curPoint <= ePoint)
				{
					fTouched = curPoint;
					curTouched = curPoint;
					curPoint++;
					fixed (bool* tarr2 = worker.TouchedArray)
					{
						bool* tarr = &tarr2[curPoint];
						while (curPoint <= ePoint)
						{
							if (*tarr)
							{
								worker.Interpolate(curTouched + 1, curPoint - 1, curTouched, curPoint, gState);
								curTouched = curPoint;
							}
							tarr++;
							curPoint++;
						}
					}

					if (curTouched == fTouched)
					{
						worker.Shift(fPoint, ePoint, curTouched);
					}
					else
					{
						worker.Interpolate(curTouched + 1, ePoint, curTouched, fTouched, gState);
						if (fTouched > 0)
							worker.Interpolate(fPoint, fTouched - 1, curTouched, fTouched, gState);
					}
				}

				curContour++;
			} while (curContour < gState.Zone1.ContourCount);


			#region Export Worker Data
			if (pXAxis)
			{
				UnsafeCopyOutX(gState.Zone1.Contour, worker.CurrentPositions);
#if WriteDebug
				foreach (F26Dot6 f in worker.CurrentPositions)
				{
					swrtr.WriteLine("End Current Position: " + f.ToString());
				}
#endif
			}
			else
			{
#if WriteDebug
				foreach (F26Dot6 f in worker.CurrentPositions)
				{
					swrtr.WriteLine("End Current Position: " + f.ToString());
				}
#endif
				UnsafeCopyOutY(gState.Zone1.Contour, worker.CurrentPositions);
			}
			#endregion

#if WriteDebug
			swrtr.Close();
#endif
		}
		#endregion

		#endregion


		#region Op-Code Methods

		/// <summary>
		/// Moves a point to the intersection of 2 lines in
		/// accordance with the ISect[] instruction.
		/// </summary>
		public static void ISect(GraphicsState gState)
		{
			int pNum, a0, a1, b0, b1;

			b1 = gState.Stack.Pop();
			b0 = gState.Stack.Pop();
			a1 = gState.Stack.Pop();
			a0 = gState.Stack.Pop();
			pNum = gState.Stack.Pop();

			F26Dot6 dx, dy, dbx, dby, dax, day;
			F26Dot6 val;
			F26Dot6 discrim;
			VecF26Dot6 V = VecF26Dot6.Zero;

			dbx = gState.zp0.Contour[b1].X - gState.zp0.Contour[b0].X;
			dby = gState.zp0.Contour[b1].Y - gState.zp0.Contour[b0].Y;

			dax = gState.zp1.Contour[a1].X - gState.zp1.Contour[a0].X;
			day = gState.zp1.Contour[a1].Y - gState.zp1.Contour[a0].Y;

			dx = gState.zp0.Contour[b0].X - gState.zp1.Contour[a0].X;
			dy = gState.zp0.Contour[b0].Y - gState.zp1.Contour[a0].Y;

			discrim = F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(dax), F26Dot6.AsLiteral(-dby), 0x40) + MulDiv(F26Dot6.AsLiteral(day), F26Dot6.AsLiteral(dbx), 0x40));

			if (F26Dot6.Abs(discrim) >= F26Dot6.FromLiteral(0x40))
			{
				val = F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(dx), F26Dot6.AsLiteral(-dby), 0x40) + MulDiv(F26Dot6.AsLiteral(dy), F26Dot6.AsLiteral(dbx), 0x40));

				V.X = F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(val), F26Dot6.AsLiteral(dax), F26Dot6.AsLiteral(discrim)));
				V.Y = F26Dot6.FromLiteral(MulDiv(F26Dot6.AsLiteral(val), F26Dot6.AsLiteral(day), F26Dot6.AsLiteral(discrim)));

				gState.zp2.Contour[pNum].X = gState.zp1.Contour[a0].X + V.X;
				gState.zp2.Contour[pNum].Y = gState.zp1.Contour[a0].Y + V.Y;
			}
			else
			{
				gState.zp2.Contour[pNum].X = (gState.zp1.Contour[a0].X +
											  gState.zp1.Contour[a1].X +
											  gState.zp0.Contour[b0].X +
											  gState.zp0.Contour[b1].X) >> 2;
				gState.zp2.Contour[pNum].Y = (gState.zp1.Contour[a0].Y +
											  gState.zp1.Contour[a1].Y +
											  gState.zp0.Contour[b0].Y +
											  gState.zp0.Contour[b1].Y) >> 2;
			}
			gState.zp2.TouchedX[pNum] = true;
			gState.zp2.TouchedY[pNum] = true;
		}

		/// <summary>
		/// Measures the distance between 2 points.
		/// </summary>
		public static F26Dot6 MeasureDistance(int pointA, int pointB, bool useOrig, GraphicsState gState)
		{
			if (useOrig)
			{
				if (gState.gep0 == 0 || gState.gep1 == 0)
				{
					return gState.DualProject(gState.zp0.OriginalContour[pointB], gState.zp1.OriginalContour[pointA]);
				}
				else 
				{
					if (gState.Scale_X == gState.Scale_Y)
					{
						return gState.DualProject(gState.zp0.OriginalContour[pointB], gState.zp1.OriginalContour[pointA]) * gState.Scale_X;
					}
					else
					{
						throw new Exception("Non-uniform scale not currently supported!");
					}
				}
			}
			else
			{
				return gState.Project(gState.zp0.Contour[pointB], gState.zp1.Contour[pointA]);
			}
		}

		/// <summary>
		/// Flips the OnCurve state of points who's
		/// indexes are popped off of the stack.
		/// </summary>
		public void LoopedFlipOnCurve()
		{
			if (gep0 == 0)
				throw new Exception("You can't set points in the Twilight zone to be on or off the curve!");
			bool[] onCurve = zp0.OnCurve;
			int ptNum;
			for (uint i = 0; i < Loop; i++)
			{
				ptNum = Stack.Pop();
				onCurve[ptNum] = !onCurve[ptNum];
			}
			Loop = 1;
		}

		/// <summary>
		/// Sets a range of points to be
		/// on the curve.
		/// </summary>
		/// <param name="end">The end index (inclusive).</param>
		/// <param name="start">The start index.</param>
		public void SetRangeOnCurve(int end, int start)
		{
			if (gep0 == 0)
				throw new Exception("You can't set points in the Twilight zone to be on or off the curve!");
			bool[] onCurve = zp0.OnCurve;
			for (int i = start; i <= end; i++)
			{
				onCurve[i] = true;
			}
		}

		/// <summary>
		/// Sets a range of points to be
		/// off the curve.
		/// </summary>
		/// <param name="end">The end index (inclusive).</param>
		/// <param name="start">The start index.</param>
		public void SetRangeOffCurve(int end, int start)
		{
			if (gep0 == 0)
				throw new Exception("You can't set points in the Twilight zone to be on or off the curve!");
			bool[] onCurve = zp0.OnCurve;
			for (int i = start; i <= end; i++)
			{
				onCurve[i] = false;
			}
		}

		/// <summary>
		/// Aligns the specified point to
		/// rp0. Other behavior specified
		/// by the AlignRP[] instruction in
		/// the spec.
		/// </summary>
		/// <param name="pointNumber">The point to align.</param>
		public void AlignToReferencePoint(int pNum)
		{
			F26Dot6 curDistance = Project(zp1.Contour[pNum], zp0.Contour[rp0]);
			Move(zp1, pNum, -curDistance);
		}

		/// <summary>
		/// Sets the coords in the 
		/// style of SCFS[].
		/// </summary>
		public void SetCoords(F26Dot6 distance, int pNum)
		{
			F26Dot6 curDist = Project(zp2.Contour[pNum]);
			Move(zp2, pNum, distance - curDist);

			if (gep2 == 0)
				zp2.OriginalContour[pNum] = zp2.Contour[pNum];
		}

		/// <summary>
		/// Gets the coords in the 
		/// style of GC[a].
		/// </summary>
		public static F26Dot6 GetCoords(int pNum, bool MovedPosition, GraphicsState gState)
		{
			if (MovedPosition)
				return gState.Project(gState.zp2.Contour[pNum]);
			else
				return gState.DualProject(gState.zp2.OriginalContour[pNum]);
		}
		#endregion


		public static void SetZonePointer(int newPtr, int zoneNum, GraphicsState gState)
		{
			switch (zoneNum)
			{
				case 0:
					gState.gep0 = newPtr;
					break;
				case 1:
					gState.gep1 = newPtr;
					break;
				case 2:
					gState.gep2 = newPtr;
					break;
				default:
					throw new Exception("Invalid Zone Number!");
			}
			gState.SetZonePtrs();
		}

		public void CallFunction(int funcNum)
		{
			//Console.WriteLine("Calling Function " + funcNum.ToString() + ".");
			Functions[funcNum](this);
		}

		/// <summary>
		/// Brings the element at the specified
		/// index to the top of the stack.
		/// </summary>
		/// <param name="indx">The item to move.</param>
		public void BringToTopOfStack(int indx)
		{
			indx--;
			LinkedStack<int> tmp = new LinkedStack<int>();
			for (uint i = 0; i < indx; i++)
			{
				tmp.Push(Stack.Pop());
			}
			int val = Stack.Pop();
			for (uint i = 0; i < indx; i++)
			{
				Stack.Push(tmp.Pop());
			}
			Stack.Push(val);
		}

		public static F26Dot6 StaticRound(F26Dot6 val, DistanceType dType, GraphicsState gState)
		{
			return gState.Round(val, dType);
		}

		/// <summary>
		/// Rounds the specified value
		/// according to the value of
		/// <see cref="RoundMode"/>.
		/// </summary>
		/// <param name="val">The value to round.</param>
		/// <param name="dType">The type of distance to round.</param>
		/// <returns>The rounded value.</returns>
		public F26Dot6 Round(F26Dot6 val, DistanceType dType)
		{
			switch (RoundMode)
			{
				case RoundingMode.Half_Grid:		return Round_HalfGrid(val);
				case RoundingMode.Grid:				return Round_Grid(val);
				case RoundingMode.Double_Grid:		return Round_DoubleGrid(val);
				case RoundingMode.Down_To_Grid:		return Round_DownToGrid(val);
				case RoundingMode.Up_To_Grid:		return Round_HalfGrid(val);
				case RoundingMode.Off:				return Round_None(val);
				case RoundingMode.Super:			return Round_Super(val);
				case RoundingMode.Super45:			return Round_Super45(val);
				default: throw new Exception("Unknown Rounding Mode!");
			}
		}

		#region Rounding Types
		private static F26Dot6 Round_None(F26Dot6 val)
		{
			return val;
		}

		private static F26Dot6 Round_HalfGrid(F26Dot6 val)
		{
			return 0.5d + F26Dot6.Round(val - 0.5d);
		}
		private static F26Dot6 Round_DoubleGrid(F26Dot6 val)
		{
			if (F26Dot6.ToDouble(val) % 2.0d < 1.0d)
				return F26Dot6.FromDouble(F26Dot6.ToDouble(val) - (F26Dot6.ToDouble(val) % 2.0d)); 
			else
				return F26Dot6.FromDouble(F26Dot6.ToDouble(val) + (2.0d - (F26Dot6.ToDouble(val) % 2.0d))); 
		}
		private static F26Dot6 Round_Grid(F26Dot6 val) { return F26Dot6.Round(val); }
		private static F26Dot6 Round_DownToGrid(F26Dot6 val) { return F26Dot6.Floor(val); }
		private static F26Dot6 Round_UpToGrid(F26Dot6 val) { return F26Dot6.Ceiling(val); }

		private F26Dot6 Round_Super(F26Dot6 distance)
		{
			F26Dot6 val;
			F26Dot6 compensation = 0;

			if (F26Dot6.ToDouble(distance) >= 0)
			{
				val = (distance - SRound_Phase + SRound_Threshold + compensation) & -SRound_Period;
				if (distance != 0 && val < 0)
					val = 0;
				val += SRound_Phase;
			}
			else
			{
				val = -((SRound_Threshold - SRound_Phase - distance + compensation) & -SRound_Period);
				if (val > 0)
					val = 0;
				val -= SRound_Phase;
			}

			return val;
		}
		private static F26Dot6 Round_Super45(F26Dot6 val)
		{
#warning TODO: Round to Super45.
//            return val;
			throw new Exception("Super45 rounding mode isn't supported yet!");
		}
		#endregion

		/// <summary>
		/// Sets the zp's depending on
		/// the values of the gep's.
		/// </summary>
		public void SetZonePtrs()
		{
			zp0 = (gep0 == 0) ? TwighlightZone : Zone1;
			zp1 = (gep1 == 0) ? TwighlightZone : Zone1;
			zp2 = (gep2 == 0) ? TwighlightZone : Zone1;
		}

	}
}
