using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Compiler.TrueType.Tables
{
	internal interface ITable
	{
		string TableTag { get; }
		void LoadTable(Stream strm, uint length, TrueTypeFont fnt);
	}
}
