using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class HorizontalHeaderTable : ITable
	{
		public string TableTag
		{
			get { return "hhea"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			fnt.HHEA_Version = rdr.ReadFixed();
			if (fnt.HHEA_Version == 1.0)
			{
				fnt.Ascender = rdr.ReadInt16();
				fnt.Decender = rdr.ReadInt16();
				fnt.LineGap = rdr.ReadInt16();
				fnt.MaxAdvanceWidth = rdr.ReadUInt16();
				fnt.MinLeftSideBearing = rdr.ReadInt16();
				fnt.MinRightSideBearing = rdr.ReadInt16();
				fnt.MaxXExtent = rdr.ReadInt16();
				fnt.CaretSlopeRise = rdr.ReadInt16();
				fnt.CaretSlopeRun = rdr.ReadInt16();
				fnt.CaretOffset = rdr.ReadInt16();
				rdr.ReadInt16();
				rdr.ReadInt16();
				rdr.ReadInt16();
				rdr.ReadInt16();
				fnt.MetricDataFormat = rdr.ReadInt16();
				fnt.NumberOfHMetrics = rdr.ReadUInt16();
			}
			else
			{
				throw new Exception("Unknown version for the 'hhea' table!");
			}
			fnt.TableRead_HHea = true;
		}
	}
}
