using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using Orvid.Graphics;
using Orvid.Compiler.TrueType.Utils;
using Orvid.Compiler.TrueType.Tables;
using Orvid.Config;
using Orvid.Compiler.TrueType.HintingVM;

namespace Orvid.Compiler.TrueType
{
	/// <summary>
	/// Represents a font in the TrueType format.
	/// </summary>
	public sealed class TrueTypeFont
	{
		#region Head Table
		public bool TableRead_Head = false;
		public double Head_Version;
		public double FontRevision;
		public uint ChecksumAdjustment;
		public ushort Flags;
		public ushort UnitsPerEm;
		public short XMin;
		public short YMin;
		public short XMax;
		public short YMax;
		public ushort MacStyle;
		public ushort LowestRecPPEM;
		public short FontDirectionHint;
		public short IndexToLocFormat;
		public short GlyphDataFormat;
		#endregion

		#region MaxP Table
		public bool TableRead_MaxP = false;
		public double MaxP_Version;
		public ushort NumberOfGlyphs;
		public ushort MaxPoints;
		public ushort MaxContours;
		public ushort MaxCompositePoints;
		public ushort MaxCompositeContours;
		public ushort MaxZones;
		public ushort MaxTwilightPoints;
		public ushort MaxStorage;
		public ushort MaxFunctionDefs;
		public ushort MaxInstructionDefs;
		public ushort MaxStackElements;
		public ushort MaxSizeOfInstructions;
		public ushort MaxComponentElements;
		public ushort MaxComponentDepth;
		#endregion

		#region HHea Table
		public bool TableRead_HHea = false;
		public double HHEA_Version;
		public short Ascender;
		public short Decender;
		public short LineGap;
		public ushort MaxAdvanceWidth;
		public short MinLeftSideBearing;
		public short MinRightSideBearing;
		public short MaxXExtent;
		public short CaretSlopeRise;
		public short CaretSlopeRun;
		public short CaretOffset;
		public short MetricDataFormat;
		public ushort NumberOfHMetrics;
		#endregion

		#region OS/2 Table
		public bool TableRead_OS2 = false;
		public ushort OS2_Version;
		public short AvgXCharWidth;
		public ushort WeightClass;
		public ushort WidthClass;
		public ushort FontStyleType;
		public short SubscriptXSize;
		public short SubscriptYSize;
		public short SubscriptXOffset;
		public short SubscriptYOffset;
		public short SuperscriptXSize;
		public short SuperscriptYSize;
		public short SuperscriptXOffset;
		public short SuperscriptYOffset;
		public short StrikeoutSize;
		public short StrikeoutPosition;
		public short FamilyClass;
		public byte[] Panose;
		public uint[] UnicodeRange = new uint[4];
		public string AchVendID = "";
		public ushort FontStyleSelection;
		public ushort FirstCharacterIndex;
		public ushort LastCharacterIndex;
		public short TypoAscender;
		public short TypoDescender;
		public short TypoLineGap;
		public ushort WinAscent;
		public ushort WinDescent;
		public uint[] CodePageRange = new uint[2];
		public short XHeight;
		public short CapHeight;
		public ushort DefaultChar;
		public ushort BreakChar;
		public ushort MaxContext;
		#endregion

		#region Loca Table
		public bool TableRead_Loca = false;
		public uint[] Offsets;
		#endregion

		#region Glyf Table
		public bool TableRead_Glyf = false;
		public Glyph[] Glyphs;
		#endregion

		#region Hmtx Table
		public bool TableRead_Hmtx = false;
		public ushort[] AdvanceWidth;
		public short[] LeftSideBearingA;
		public short[] LeftSideBearingB;
		#endregion

		#region CMap Table
		public bool TableRead_CMap = false;
		public ushort CMap_Version;
		public ushort CMapTableCount;
		public List<CMapSubTable> CMapTables = new List<CMapSubTable>();
		public CMapSubTable ActiveCharMapTable;
		#endregion

		#region Cvt Table
		public bool TableRead_Cvt = false;
		public uint[] CvtValues;
		#endregion

		#region Fpgm Table
		public bool TableRead_Fpgm = false;
		public byte[] FpgmProgram;
		#endregion

		#region Prep Table
		public bool TableRead_Prep = false;
		public byte[] PrepProgram;
		#endregion

		public void Load(Stream fil)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(fil);
			double sfntVersion = rdr.ReadFixed();
			if (sfntVersion != 1.0)
				throw new Exception("Unknown format version!");
			ushort tblCount = rdr.ReadUInt16();
			ushort searchRange = rdr.ReadUInt16();
			ushort entrySelector = rdr.ReadUInt16();
			ushort rangeShift = rdr.ReadUInt16();
			List<TableRecordEntry> entries = new List<TableRecordEntry>();
			for (uint i = 0; i < tblCount; i++)
			{
				TableRecordEntry entry = new TableRecordEntry();
				entry.Tag = rdr.ReadASCIIChars(4);
				entry.Checksum = rdr.ReadUInt32();
				entry.Offset = rdr.ReadUInt32();
				entry.Length = rdr.ReadUInt32();
				entries.Add(entry);
			}

			foreach (TableRecordEntry entry in entries)
			{
				ITable tbl = TableRegister.GetTable(entry.Tag);
				rdr.BaseStream.Flush();
				rdr.BaseStream.Position = entry.Offset;
				rdr.BaseStream.Flush();
				tbl.LoadTable(rdr.BaseStream, entry.Length, this);
			}

			while (TableRegister.DelayedTables.Count > 0)
			{
				TableRecordEntry entry = TableRegister.DelayedTables.Dequeue();
				ITable tbl = TableRegister.GetTable(entry.Tag);
				rdr.BaseStream.Position = entry.Offset;
				rdr.BaseStream.Flush();
				tbl.LoadTable(rdr.BaseStream, entry.Length, this);
			}
		}

		/// <summary>
		/// Saves this font to file.
		/// </summary>
		/// <param name="fileName">The filename to save it to.</param>
		/// <param name="emitDebug">True if debug information should be emitted for it.</param>
		public void Save(string fileName, bool emitDebug)
		{
			IRAssembly asmbly = new IRAssembly(fileName, emitDebug, this);
			asmbly.Read();
			asmbly.Emit();
		}

		/// <summary>
		/// Gets the left side bearing of the
		/// specified glyph.
		/// </summary>
		/// <param name="glyphIndex">The glyph to get the left side bearing of.</param>
		/// <returns>The left side bearing.</returns>
		public int GetLeftSideBearing(int glyphIndex)
		{
			if (glyphIndex > AdvanceWidth.Length)
			{
				if (glyphIndex > AdvanceWidth.Length + LeftSideBearingB.Length)
				{
					throw new Exception("This shouldn't occur!");
				}
				else
				{
					return LeftSideBearingB[glyphIndex - AdvanceWidth.Length];
				}
			}
			else
			{
				return LeftSideBearingA[glyphIndex];
			}
		}

		/// <summary>
		/// Gets the advance width of the specified
		/// glyph.
		/// </summary>
		/// <param name="glyphIndex">The glyph to get the advance width of.</param>
		/// <returns>The advance width.</returns>
		public int GetAdvanceWidth(int glyphIndex)
		{
			if (glyphIndex > AdvanceWidth.Length)
			{
				return AdvanceWidth[AdvanceWidth.Length - 1];
			}
			else
			{
				return AdvanceWidth[glyphIndex];
			}
		}

		//public Image RenderString(double SizeInPoints, string str)
		//{
		//    char[] chars = str.ToCharArray();
		//    Glyph[] glyfs = new Glyph[chars.Length];
		//    Image[] Imgs = new Image[chars.Length];
		//    int height = 0;
		//    const int LetterSpacing = 0;
		//    int TotalWidth = LetterSpacing;
		//    for (uint i = 0; i < chars.Length; i++)
		//    {
		//        glyfs[i] = Glyphs[ActiveCharMapTable.GetGlyphIndex(chars[i])];
		//        Imgs[i] = glyfs[i].GetRendering(SizeInPoints);
		//    }

		//    int decenderHeight = 0;
		//    // Bounds computing pass.
		//    for (uint i = 0; i < Imgs.Length; i++)
		//    {
		//        Image im = Imgs[i];
		//        if (im.Height > height)
		//            height = im.Height + 4;
		//        TotalWidth += (int)(
		//            Math.Ceiling(
		//                UnitsToPixel(
		//                    SizeInPoints, 
		//                    AdvanceWidth[ActiveCharMapTable.GetGlyphIndex(chars[i])]
		//                )
		//            + SimpleGlyph.CharacterSide * 3)
		//        );

		//    }

		//    Image o = new Image(TotalWidth, height);
		//    o.Clear(Colors.White);
		//    int curLocX = 2;
		//    int yPos = 0;
		//    for (uint i = 0; i < chars.Length; i++)
		//    {
		//        yPos = height - Imgs[i].Height - 2;
		//        o.DrawImage(new Vec2(curLocX, yPos), Imgs[i]);
		//        curLocX += (int)(
		//            Math.Ceiling(
		//                    UnitsToPixel(SizeInPoints,
		//                    AdvanceWidth[ActiveCharMapTable.GetGlyphIndex(chars[i])])
		//                )
		//        );
				
		//    }

		//    return o;
		//}

		//public double UnitsToPixel(double SizeInPoints, double val)
		//{
		//    return ((val * SizeInPoints * FontSettings.HorizontalDPI) / (UnitsPerEm * 72));
		//}

	}
}
