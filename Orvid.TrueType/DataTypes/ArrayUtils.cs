using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	public static class ArrayUtils
	{
		public static unsafe void AssignValueToGlyphArray(Glyph[] arr, Glyph value)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = value;
			}
		}
	}
}
