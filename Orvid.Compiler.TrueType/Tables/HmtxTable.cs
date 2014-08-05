using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class HmtxTable : ITable
	{
		public string TableTag
		{
			get { return "hmtx"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			if (!fnt.TableRead_MaxP || !fnt.TableRead_HHea)
			{
				TableRecordEntry entry = new TableRecordEntry();
				entry.Offset = (uint)strm.Position;
				entry.Tag = this.TableTag;
				entry.Length = length;
				TableRegister.DelayTable(entry);
				return;
			}
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);

			fnt.AdvanceWidth = new ushort[fnt.NumberOfHMetrics];
			fnt.LeftSideBearingA = new short[fnt.NumberOfHMetrics];
			fnt.LeftSideBearingB = new short[fnt.NumberOfGlyphs - fnt.NumberOfHMetrics];

			for (uint i = 0; i < fnt.AdvanceWidth.Length; i++)
			{
				fnt.AdvanceWidth[i] = rdr.ReadUInt16();
				fnt.LeftSideBearingA[i] = rdr.ReadInt16();
			}

			for (uint i = 0; i < fnt.LeftSideBearingB.Length; i++)
			{
				fnt.LeftSideBearingB[i] = rdr.ReadInt16();
			}

			fnt.TableRead_Hmtx = true;
		}
	}
}
