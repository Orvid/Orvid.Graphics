using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class PrepTable : ITable
	{
		public string TableTag
		{
			get { return "prep"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			fnt.PrepProgram = new byte[length];
			for (uint i = 0; i < length; i++)
			{
				fnt.PrepProgram[i] = rdr.ReadByte();
			}
			fnt.TableRead_Prep = true;
		}
	}
}
