using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics
{
	/// <summary>
	/// Various utility methods.
	/// </summary>
	internal static class Utils
	{

		/// <summary>
		/// Returns the minimum of the 3
		/// numbers specified.
		/// </summary>
		/// <param name="a">The first number.</param>
		/// <param name="b">The second number.</param>
		/// <param name="c">The third number.</param>
		/// <returns>The smallest of the 3 numbers.</returns>
		public static int GetMin(int a, int b, int c)
		{
			if (a <= b)
				if (a <= c)
					return a;
			if (b <= a)
				if (b <= c)
					return b;
			if (c <= a)
				if (c <= b)
					return c;

			return int.MaxValue;
		}

		/// <summary>
		/// Returns the maximum of the 3
		/// numbers specified.
		/// </summary>
		/// <param name="a">The first number.</param>
		/// <param name="b">The second number.</param>
		/// <param name="c">The third number.</param>
		/// <returns>The biggest of the 3 numbers.</returns>
		public static int GetMax(int a, int b, int c)
		{
			if (a >= b)
				if (a >= c)
					return a;
			if (b >= a)
				if (b >= c)
					return b;
			if (c >= a)
				if (c >= b)
					return c;

			return int.MaxValue;
		}
	}
}
