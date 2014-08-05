using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class VDMXTable : ITable
	{
		public string TableTag
		{
			get { return "VDMX"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{

		}
	}
}
