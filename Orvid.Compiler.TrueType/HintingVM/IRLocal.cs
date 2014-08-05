using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Compiler.TrueType.HintingVM
{
	public enum LocalType
	{
		Integer,
		F26Dot6
	}

	/// <summary>
	/// Represents a local variable.
	/// </summary>
	public class IRLocal
	{
		public LocalType Type;

		public IRLocal(LocalType type)
		{
			this.Type = type;
		}
	}
}
