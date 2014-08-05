using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class LocaTable : ITable
	{
		public string TableTag
		{
			get { return "loca"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			if (!fnt.TableRead_Head || !fnt.TableRead_MaxP)
			{
				TableRecordEntry entry = new TableRecordEntry();
				entry.Offset = (uint)strm.Position;
				entry.Tag = TableTag;
				entry.Length = length;
				TableRegister.DelayTable(entry);
				return;
			}
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			bool IsLongIndex = (fnt.IndexToLocFormat != 0);
			int glyphCount = fnt.NumberOfGlyphs + 1;
			fnt.Offsets = new uint[glyphCount];
			if (IsLongIndex)
			{
				for (uint i = 0; i < glyphCount; i++)
				{
					fnt.Offsets[i] = rdr.ReadUInt32();
				}
			}
			else
			{
				for (uint i = 0; i < glyphCount; i++)
				{
					fnt.Offsets[i] = (uint)(rdr.ReadUInt16() * 2);
				}
			}
			fnt.TableRead_Loca = true;
		}
	}
}
