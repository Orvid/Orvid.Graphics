using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	/// <summary>
	/// A set of extra math functions.
	/// </summary>
	public static class MathUtils
	{
		#region Factorial Table
		private static readonly double[] FactorialTable = new double[] 
		{
			1,
			1,
			2,
			6,
			24,
			120,
			720.0,
			5040.0,
			40320.0,
			362880.0,
			3628800.0,
			39916800.0,
			479001600.0,
			6227020800.0,
			87178291200.0,
			1307674368000.0,
			20922789888000.0,
			355687428096000.0,
			6402373705728000.0,
			121645100408832000.0,
			2432902008176640000.0,
			51090942171709440000.0,
			1124000727777607680000.0,
			25852016738884976640000.0,
			620448401733239439360000.0,
			15511210043330985984000000.0,
			403291461126605635584000000.0,
			10888869450418352160768000000.0,
			304888344611713860501504000000.0,
			8841761993739701954543616000000.0,
			265252859812191058636308480000000.0,
			8222838654177922817725562880000000.0,
			263130836933693530167218012160000000.0,
		};
		#endregion

		/// <summary>
		/// Calculate the factorial of the
		/// given value.
		/// </summary>
		/// <param name="val">
		/// The value to calculate the factorial
		/// of.
		/// </param>
		/// <returns>The calculated factorial.</returns>
		public static double Factorial(double val)
		{
			if (val < 0)
				throw new Exception("Value must be >= 0!");
			if (val > 32)
				throw new Exception("Value is too large to represent!");
			return FactorialTable[(uint)val];
		}

	}
}
