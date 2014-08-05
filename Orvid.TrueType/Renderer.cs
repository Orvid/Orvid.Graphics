using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Orvid.TrueType
{
	public class Renderer
	{
		private const int MaxBezier = 32;
		private const int PixelBits = 6;

		private enum RenderState
		{
			Unknown = 0,
			Ascending,
			Descending,
			Flat,
		}

		[Flags]
		private enum ProfileFlags
		{
			FlowUp = 0x08,
			OvershootTop = 0x10,
			OvershootBottom = 0x20,
		}

		private class Profile
		{
			public F26Dot6 X;
			public Profile Link;
			public int Offset;
			public ProfileFlags Flags;
			public int Height;
			public int Start;
			public uint CountL;
			public Profile Next;
		}

		private struct BBand
		{
			public short YMin;
			public short YMax;
		}

		/// <summary>
		/// Renders black/white text.
		/// NEVER PASS THIS AS AN ARGUMENT!
		/// If you do, you will be passing
		/// over 1mb of data around.
		/// </summary>
		private unsafe struct BWorker
		{
			private const int RenderPoolSize = 1024 * 1024;
			private fixed byte RenderPool[RenderPoolSize];
			public int Precision_Bits;
			public int Precision;
			public int Precision_Half;
			public int Precision_Shift;
			public int Precision_Step;
			public int Precision_Jitter;
			public int ScaleShift;
			public int BufferTop;
			public int TurnCount;
			public VecF26Dot6 Arc;
			public ushort Width;
			public byte[] BitmapTarget;
			public F26Dot6 LastX;
			public F26Dot6 LastY;
			public F26Dot6 MinY;
			public F26Dot6 MaxY;
			public ushort ProfileCount;
			public bool Fresh;
			public bool Joint;
			public Profile CurrentProfile;
			public Profile Profiles_Head;
			public Profile Profiles_First;
			public RenderState State;
			public int TraceOffset;
			public short TraceIncrement;
			public short Grey_MinX;
			public short Grey_MaxX;
			public byte DropOutControl;
			public bool SecondPass;
			public fixed ulong Arcs_Fixed[ArcsLength];
			public fixed uint BandStack_Fixed[BandStackLength];
			private const int ArcsLength = 3 * MaxBezier + 1;
			private const int BandStackLength = 16;
			public int BandTop;
			public Outline Outline;

			public void SetPrecisionHigh()
			{
				this.Precision_Bits = 12;
				this.Precision_Step = 256;
				this.Precision_Jitter = 30;
				this.Precision = 1 << Precision_Bits;
				this.Precision_Half = Precision >> 1;
				this.Precision_Shift = Precision_Bits - PixelBits;
			}
			public void SetPrecisionLow()
			{
				this.Precision_Bits = 6;
				this.Precision_Step = 32;
				this.Precision_Jitter = 2;
				this.Precision = 1 << Precision_Bits;
				this.Precision_Half = Precision >> 1;
				this.Precision_Shift = Precision_Bits - PixelBits;
			}

			public unsafe void RenderGlyph()
			{
				this.ScaleShift = Precision_Shift;
				BandTop = 0;
				fixed (ulong* Arcs8 = Arcs_Fixed)
				{
					fixed (uint* BandStack8 = BandStack_Fixed)
					{
						VecF26Dot6* Arcs = (VecF26Dot6*)Arcs8;
						BBand* BandStack = (BBand*)BandStack8;
						BandStack[0].YMin = 0;
						BandStack[0].YMax = (short)(Outline.MaxY - 1);
						this.BitmapTarget = new byte[Outline.MaxX * Outline.MaxY];
						Render_Single_Pass(Arcs, BandStack);

					}
				}
			}

			private unsafe void Render_Single_Pass(VecF26Dot6* Arcs, BBand* BandStack)
			{
				int i, j, k;
				while (BandTop >= 0)
				{
					MinY = BandStack[BandTop].YMin * Precision;
					MaxY = BandStack[BandTop].YMax * Precision;
					ConvertGlyph(Arcs, BandStack);
				}
			}

			private unsafe void ConvertGlyph(VecF26Dot6* Arcs, BBand* BandStack)
			{

			}
		}

		public unsafe void TestMe()
		{
			int sz = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BWorker));
			BWorker worker = new BWorker();
			//VecF26Dot6 v = ((VecF26Dot6*)worker.Arcs)[0];
			throw new Exception();
		}

	}
}
