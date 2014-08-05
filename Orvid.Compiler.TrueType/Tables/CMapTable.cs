using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class CMapTable : ITable
	{
		public string TableTag
		{
			get { return "cmap"; }
		}
		
		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			long basePosition = strm.Position;
			fnt.CMap_Version = rdr.ReadUInt16();
			if (fnt.CMap_Version == 0)
			{
				fnt.CMapTableCount = rdr.ReadUInt16();
				for (uint i = 0; i < fnt.CMapTableCount; i++)
				{
					CMapSubTable sTable = new CMapSubTable();
					sTable.PlatformID = rdr.ReadUInt16();
					sTable.EncodingID = rdr.ReadUInt16();
					sTable.Offset = rdr.ReadUInt32();
					fnt.CMapTables.Add(sTable);
				}
				long curPos = strm.Position;
				foreach(CMapSubTable tbl in fnt.CMapTables)
				{
					strm.Position = basePosition + tbl.Offset;
					strm.Flush();
					tbl.LoadSubTable(strm);
					// In general, the highest format
					// number is the table that has 
					// the most character information.
					if (tbl.PlatformID == 0 && (fnt.ActiveCharMapTable == null || tbl.Format > fnt.ActiveCharMapTable.Format))
					{
						fnt.ActiveCharMapTable = tbl;
					}
				}
			}
			else
			{
				throw new Exception("Unknown version for the 'cmap' table!");
			}
			fnt.TableRead_CMap = true;
		}
	}
}
