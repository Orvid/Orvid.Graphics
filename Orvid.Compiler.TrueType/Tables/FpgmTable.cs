using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class FpgmTable : ITable
	{
		public string TableTag
		{
			get { return "fpgm"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			fnt.FpgmProgram = new byte[length];
			for (uint i = 0; i < length; i++)
			{
				fnt.FpgmProgram[i] = rdr.ReadByte();
			}
			fnt.TableRead_Fpgm = true;
		}
	}
}
