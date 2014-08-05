using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType
{
	public class CMapSubTable
	{
		public ushort PlatformID;
		public ushort EncodingID;
		public uint Offset;

		public ushort Format;
		private TableFormat FormatImplementation;
		public ushort Length;
		public ushort Language;

		public void LoadSubTable(Stream strm)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			this.Format = rdr.ReadUInt16();
			this.Length = rdr.ReadUInt16();
			this.Language = rdr.ReadUInt16();

			// Best reference for formats:
			// http://www.microsoft.com/typography/otspec/cmap.htm
			switch (this.Format)
			{
				case 0:
					this.FormatImplementation = new Format0();
					break;
				case 4:
					this.FormatImplementation = new Format4();
					break;

				default:
					throw new Exception("Unsupported sub-table format!");
			}
			this.FormatImplementation.Read(rdr);
		}

		public uint GetGlyphIndex(char c)
		{
			return this.FormatImplementation.GetGlyphIndex(c);
		}

		public int[] GetMappedGlyphIndexes()
		{
			return this.FormatImplementation.GetMappedGlyphIndexes();
		}

		private abstract class TableFormat
		{
			public abstract void Read(BigEndianBinaryReader rdr);
			public abstract uint GetGlyphIndex(char c);
			public abstract int[] GetMappedGlyphIndexes();
		}

		#region Format 0
		private class Format0 : TableFormat
		{
			public byte[] GlyphIDArray;

			public override void Read(BigEndianBinaryReader rdr)
			{
				this.GlyphIDArray = rdr.ReadBytes(256);
			}

			public override uint GetGlyphIndex(char c)
			{
				if (((int)c) > 256)
				{
					return 0;
				}
				else
				{
					return GlyphIDArray[(int)c];
				}
			}

			#region GetMappedGlyphIndexes
			public unsafe override int[] GetMappedGlyphIndexes()
			{
				int[] darr3 = new int[256];
				fixed (int* darr2 = darr3)
				{
					fixed (byte* sarr1 = GlyphIDArray)
					{
						byte* sarr = sarr1;
						int* darr = darr2;
						for (int i = 0; i < 256; i++)
						{
							*darr = *sarr;
							darr++;
							sarr++;
						}
					}
				}
				return darr3;
			}
			#endregion
		}
		#endregion

		#region Format 4
		private class Format4 : TableFormat
		{
			public int SegmentCount;
			public ushort[] EndCount, StartCount, IDRangeOffset;
			public short[] IDDelta;
			public byte[] GlyphIDArray;

			public override void Read(BigEndianBinaryReader rdr)
			{
				this.SegmentCount = rdr.ReadUInt16() / 2;
				rdr.ReadBytes(6);
				EndCount = new ushort[SegmentCount];
				for (uint i = 0; i < SegmentCount; i++)
				{
					EndCount[i] = rdr.ReadUInt16();
				}
				if (rdr.ReadUInt16() != 0)
					throw new Exception("Reserved padding wasn't zero!");
				StartCount = new ushort[SegmentCount];
				for (uint i = 0; i < SegmentCount; i++)
				{
					StartCount[i] = rdr.ReadUInt16();
				}
				IDDelta = new short[SegmentCount];
				for (uint i = 0; i < SegmentCount; i++)
				{
					IDDelta[i] = rdr.ReadInt16();
				}
				IDRangeOffset = new ushort[SegmentCount];
				for (uint i = 0; i < SegmentCount; i++)
				{
					IDRangeOffset[i] = rdr.ReadUInt16();
				}
				int max = 0;
				int mSize = 0;
				for(int i = 0; i < SegmentCount; i++)
				{
					if (IDRangeOffset[i] > max)
					{
						max = IDRangeOffset[i];
						mSize = ((EndCount[i] - StartCount[i] + 1) << 1) + max;
					}
				}


				GlyphIDArray = rdr.ReadBytes(mSize);

			}

			public override uint GetGlyphIndex(char c)
			{
				uint cID = (uint)c;
				int Index = -1;


				for (uint i = 0; i < StartCount.Length; i++)
				{
					if (StartCount[i] >= cID)
						break;
					else
						Index++;
				}

				if (Index < 0 || EndCount[Index] < cID)
					return 0; // This means the character isn't defined.

				if (IDRangeOffset[Index] != 0)
				{
					uint offset = (uint)(IDRangeOffset[Index] + (cID - StartCount[Index]));
					int gl = ((GlyphIDArray[offset] << 8) | GlyphIDArray[offset + 1]);
					return (uint)gl;
				}
				else
				{
					return (uint)(cID + IDDelta[Index]);
				}
			}

			#region GetMappedGlyphIndexes
			public override int[] GetMappedGlyphIndexes()
			{
				int[] darr = new int[ushort.MaxValue + 1];
				for (int i = 0; i < StartCount.Length; i++)
				{
					if (IDRangeOffset[i] != 0)
					{
						int bOffset = IDRangeOffset[i];
						int sCount = StartCount[i];
						int eCount = EndCount[i];
						for (int i2 = sCount; i2 <= eCount; i2++)
						{
							int offset = bOffset + ((i2 - sCount) << 1);
							int gl = ((GlyphIDArray[offset] << 8) | GlyphIDArray[offset + 1]);
							darr[i2] = gl;
						}
					}
					else
					{
						int delta = IDDelta[i];
						int eCount = EndCount[i];
						for (int i2 = StartCount[i]; i2 <= eCount; i2++)
						{
							darr[i2] = i2 + delta;
						}
					}
				}
				return darr;
			}
			#endregion

		}
		#endregion


		public override string ToString()
		{
			return "Platform: " + PlatformID.ToString() + ", Encoding: " + EncodingID.ToString() + ", Offset: " + Offset.ToString();
		}
	}
}
