using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class DSigTable : ITable
	{
		public string TableTag
		{
			get { return "DSIG"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{

		}
	}
}
