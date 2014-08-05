using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Orvid.Compiler.TrueType.Utils;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class CvtTable : ITable
	{
		public string TableTag
		{
			get { return "cvt "; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{
			BigEndianBinaryReader rdr = new BigEndianBinaryReader(strm);
			uint ValueCount = length / 2;
			fnt.CvtValues = new uint[ValueCount];
			for (uint i = 0; i < ValueCount; i++)
			{
				fnt.CvtValues[i] = rdr.ReadUInt16();
			}
			fnt.TableRead_Cvt = true;
		}
	}
}
