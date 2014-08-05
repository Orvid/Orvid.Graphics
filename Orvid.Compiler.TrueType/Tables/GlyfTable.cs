using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class GlyfTable : ITable
	{
		public string TableTag
		{
			get { return "glyf"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			if (!fnt.TableRead_MaxP || !fnt.TableRead_Loca)
			{
				TableRecordEntry entry = new TableRecordEntry();
				entry.Offset = (uint)strm.Position;
				entry.Tag = this.TableTag;
				entry.Length = length;
				TableRegister.DelayTable(entry);
				return;
			}
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			fnt.Glyphs = new Glyph[fnt.NumberOfGlyphs];
			for (uint i = 0; i < fnt.NumberOfGlyphs; i++)
			{
				if ((i > 0) && (fnt.Offsets[i - 1] == fnt.Offsets[i]))
				{
					fnt.Glyphs[i] = fnt.Glyphs[i - 1];
				}
				else
				{
					long oldPos = strm.Position;
					strm.Position = fnt.Offsets[i] + oldPos;
					strm.Flush();
					short contourCount = rdr.ReadInt16();
					if (contourCount > 0)
					{
						fnt.Glyphs[i] = new SimpleGlyph(strm, contourCount, fnt, i);
					}
					else
					{

					}
					strm.Position = oldPos;
					strm.Flush();
				}
			}
			fnt.TableRead_Glyf = true;
		}
	}
}
