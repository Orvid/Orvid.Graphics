using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Compiler.TrueType
{
	/// <summary>
	/// Represents an entry in the table
	/// record.
	/// </summary>
	public struct TableRecordEntry
	{
		public string Tag;
		public uint Checksum;
		public uint Offset;
		public uint Length;

		public override string ToString()
		{
			return this.Tag + " @ 0x" + Offset.ToString("x8");
		}
	}
}
