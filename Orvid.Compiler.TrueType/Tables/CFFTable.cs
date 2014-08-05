using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class CFFTable : ITable
	{
		public string TableTag
		{
			get { return "CFF "; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{

		}
	}
}
