using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	/// <summary>
	/// Represents a fixed point number 
	/// with 14 bits of fraction.
	/// </summary>
	public struct F2Dot14
	{
		public static readonly F2Dot14 Zero = FromLiteral(0);
		public static readonly F2Dot14 One = FromLiteral(1 << 15);
		public static readonly F2Dot14 NegativeOne = FromLiteral(-1 << 14);
		internal short value;

		private const double A16384th = 1.0d / 16384d;

		public static double ToDouble(F2Dot14 val)
		{
			return (val.value * A16384th);
		}

		public static F2Dot14 FromDouble(double d)
		{
			return F2Dot14.FromLiteral((int)Math.Round(d / A16384th));
		}

		//public static F2Dot14 FromF2Dot14(int val)
		//{
		//    return FromDouble((double)val * A8192nd);
		//}

		//public static int AsLiteralF2Dot14(F2Dot14 val)
		//{
		//    return (int)(((int)F2Dot14.ToDouble(val)) * 8192d);
		//}


        public static int AsLiteral(F2Dot14 val)
        {
            return val.value;
        }

		public static F2Dot14 FromLiteral(int v)
		{
			F2Dot14 val = new F2Dot14();
			val.value = (short)v;
			return val;
		}

		public override string ToString()
		{
			if (value < 0)
				return "-" + ((int)Math.Floor(((double)-this.value * A16384th))).ToString() + "." + (-(int)(((this.value * A16384th) - Math.Ceiling(((double)this.value * A16384th))) / A16384th)).ToString() + "/16384";
			return ((int)Math.Floor(((double)this.value * A16384th))).ToString() + "." + ((int)(((this.value * A16384th) - Math.Floor(((double)this.value * A16384th))) / A16384th)).ToString() + "/16384";
		}

		#region Mathmatical Operators

		public static F26Dot6 operator *(F2Dot14 a, F26Dot6 b)
		{
			return b * a;
		}

		public static F26Dot6 operator *(F26Dot6 pA, F2Dot14 pB)
		{
			int a = F26Dot6.AsLiteral(pA);
			int b = F2Dot14.AsLiteral(pB);
			int sign = a ^ b;
			uint low, mid, high;
			if (a < 0)
				a = -a;
			if (b < 0)
				b = -b;

			low = ((uint)a & 0xFFFFU) * (uint)b;
			mid = (uint)(((a >> 16) & 0xFFFFU) * (uint)b);
			high = mid >> 16;
			mid = (mid << 16) + (1 << 13);
			low += mid;
			if (low < mid)
				high++;
			mid = (low >> 14) | (high << 18);
			return F26Dot6.FromLiteral(sign >= 0 ? (int)mid : -(int)mid);
		}

		public static bool operator ==(F2Dot14 a, F2Dot14 b)
		{
			return a.value == b.value;
		}
		
		public static bool operator !=(F2Dot14 a, F2Dot14 b)
		{
			return a.value != b.value;
		}

		public static F2Dot14 Abs(F2Dot14 val)
		{
			if (val.value < 0)
			{
				return F2Dot14.FromLiteral(-val.value);
			}
			return F2Dot14.FromLiteral(val.value);
		}
		#endregion

		#region Implicit Conversion Operators
		public static implicit operator F2Dot14(double val) { return FromDouble(val); }
		#endregion

		#region Other Members
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj is F2Dot14)
				return value == ((F2Dot14)obj).value;
			else
				return false;
		}
		#endregion

	}
}
