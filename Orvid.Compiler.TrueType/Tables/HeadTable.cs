using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class HeadTable : ITable
	{
		public string TableTag
		{
			get { return "head"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			fnt.Head_Version = rdr.ReadFixed();
			if (fnt.Head_Version == 1.0)
			{
				fnt.FontRevision = rdr.ReadFixed();
				fnt.ChecksumAdjustment = rdr.ReadUInt32();
				if (rdr.ReadUInt32() != 0x5F0F3CF5)
				{
					throw new Exception("Magic number is incorrect!");
				}
				fnt.Flags = rdr.ReadUInt16();
				fnt.UnitsPerEm = rdr.ReadUInt16();
				rdr.ReadUInt64(); // created date
				rdr.ReadUInt64(); // modified date
				fnt.XMin = rdr.ReadInt16();
				fnt.YMin = rdr.ReadInt16();
				fnt.XMax = rdr.ReadInt16();
				fnt.YMax = rdr.ReadInt16();
				fnt.MacStyle = rdr.ReadUInt16();
				fnt.LowestRecPPEM = rdr.ReadUInt16();
				fnt.FontDirectionHint = rdr.ReadInt16();
				fnt.IndexToLocFormat = rdr.ReadInt16();
				fnt.GlyphDataFormat = rdr.ReadInt16();
			}
			else
			{
				throw new Exception("Unknown version for the 'head' table!");
			}
			fnt.TableRead_Head = true;
		}
	}
}
