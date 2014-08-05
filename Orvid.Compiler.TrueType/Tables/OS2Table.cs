using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class OS2Table : ITable
	{
		public string TableTag
		{
			get { return "OS/2"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			fnt.OS2_Version = rdr.ReadUInt16();
#warning TODO: Support versions 0, 2, and 3 of this table.
			if (fnt.OS2_Version == 4)
			{
				fnt.AvgXCharWidth = rdr.ReadInt16();
				fnt.WeightClass = rdr.ReadUInt16();
				fnt.WidthClass = rdr.ReadUInt16();
				fnt.FontStyleType = rdr.ReadUInt16();
				fnt.SubscriptXSize = rdr.ReadInt16();
				fnt.SubscriptYSize = rdr.ReadInt16();
				fnt.SubscriptXOffset = rdr.ReadInt16();
				fnt.SubscriptYOffset = rdr.ReadInt16();
				fnt.SuperscriptXSize = rdr.ReadInt16();
				fnt.SuperscriptYSize = rdr.ReadInt16();
				fnt.SuperscriptXOffset = rdr.ReadInt16();
				fnt.SuperscriptYOffset = rdr.ReadInt16();
				fnt.StrikeoutSize = rdr.ReadInt16();
				fnt.StrikeoutPosition = rdr.ReadInt16();
				fnt.FamilyClass = rdr.ReadInt16();
				fnt.Panose = rdr.ReadBytes(10);
				fnt.UnicodeRange[0] = rdr.ReadUInt32();
				fnt.UnicodeRange[1] = rdr.ReadUInt32();
				fnt.UnicodeRange[2] = rdr.ReadUInt32();
				fnt.UnicodeRange[3] = rdr.ReadUInt32();
				fnt.AchVendID = rdr.ReadASCIIChars(4);
				fnt.FontStyleSelection = rdr.ReadUInt16();
				fnt.FirstCharacterIndex = rdr.ReadUInt16();
				fnt.LastCharacterIndex = rdr.ReadUInt16();
				fnt.TypoAscender = rdr.ReadInt16();
				fnt.TypoDescender = rdr.ReadInt16();
				fnt.TypoLineGap = rdr.ReadInt16();
				fnt.WinAscent = rdr.ReadUInt16();
				fnt.WinDescent = rdr.ReadUInt16();
				fnt.CodePageRange[0] = rdr.ReadUInt32();
				fnt.CodePageRange[1] = rdr.ReadUInt32();
				fnt.XHeight = rdr.ReadInt16();
				fnt.CapHeight = rdr.ReadInt16();
				fnt.DefaultChar = rdr.ReadUInt16();
				fnt.BreakChar = rdr.ReadUInt16();
				fnt.MaxContext = rdr.ReadUInt16();
			}
			else if (fnt.OS2_Version == 1)
			{
				fnt.AvgXCharWidth = rdr.ReadInt16();
				fnt.WeightClass = rdr.ReadUInt16();
				fnt.WidthClass = rdr.ReadUInt16();
				fnt.FontStyleType = rdr.ReadUInt16();
				fnt.SubscriptXSize = rdr.ReadInt16();
				fnt.SubscriptYSize = rdr.ReadInt16();
				fnt.SubscriptXOffset = rdr.ReadInt16();
				fnt.SubscriptYOffset = rdr.ReadInt16();
				fnt.SuperscriptXSize = rdr.ReadInt16();
				fnt.SuperscriptYSize = rdr.ReadInt16();
				fnt.SuperscriptXOffset = rdr.ReadInt16();
				fnt.SuperscriptYOffset = rdr.ReadInt16();
				fnt.StrikeoutSize = rdr.ReadInt16();
				fnt.StrikeoutPosition = rdr.ReadInt16();
				fnt.FamilyClass = rdr.ReadInt16();
				fnt.Panose = rdr.ReadBytes(10);
				fnt.UnicodeRange[0] = rdr.ReadUInt32();
				fnt.UnicodeRange[1] = rdr.ReadUInt32();
				fnt.UnicodeRange[2] = rdr.ReadUInt32();
				fnt.UnicodeRange[3] = rdr.ReadUInt32();
				fnt.AchVendID = rdr.ReadASCIIChars(4);
				fnt.FontStyleSelection = rdr.ReadUInt16();
				fnt.FirstCharacterIndex = rdr.ReadUInt16();
				fnt.LastCharacterIndex = rdr.ReadUInt16();
				fnt.TypoAscender = rdr.ReadInt16();
				fnt.TypoDescender = rdr.ReadInt16();
				fnt.TypoLineGap = rdr.ReadInt16();
				fnt.WinAscent = rdr.ReadUInt16();
				fnt.WinDescent = rdr.ReadUInt16();
				fnt.CodePageRange[0] = rdr.ReadUInt32();
				fnt.CodePageRange[1] = rdr.ReadUInt32();
			}
			else
			{
				throw new Exception("Unknown version for the 'OS/2' table!");
			}
			fnt.TableRead_OS2 = true;
		}
	}
}
