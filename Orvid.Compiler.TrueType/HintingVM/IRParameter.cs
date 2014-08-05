using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Compiler.TrueType.HintingVM
{
	public enum ParameterType
	{
		Integer,
		F26Dot6,
	}

	/// <summary>
	/// Represents a parameter.
	/// </summary>
	public class IRParameter
	{
		public ParameterType Type;

		public IRParameter(ParameterType type)
		{
			this.Type = type;
		}
	}
}
