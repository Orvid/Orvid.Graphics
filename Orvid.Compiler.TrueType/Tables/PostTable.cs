using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Compiler.TrueType.Tables
{
	internal class PostTable : ITable
	{
		public string TableTag
		{
			get { return "post"; }
		}

		public void LoadTable(Stream strm, uint length, TrueTypeFont fnt)
		{

		}
	}
}
