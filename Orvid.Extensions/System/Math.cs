using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    /// <summary>
    /// Contains various extensions to <see cref="System.Math"/>.
    /// </summary>
    public static class MathExtensions
    {
        #region Internal things

        private const ulong QuadhighestBit = 1UL << 63;

        private static int Quadnlz(ulong x)
        {
            //Future work: might be faster with a huge, explicit nested if tree, or use of an 256-element per-byte array.            

            int n;

            if (x == 0) return (64);
            n = 0;
            if (x <= 0x00000000FFFFFFFF) { n = n + 32; x = x << 32; }
            if (x <= 0x0000FFFFFFFFFFFF) { n = n + 16; x = x << 16; }
            if (x <= 0x00FFFFFFFFFFFFFF) { n = n + 8; x = x << 8; }
            if (x <= 0x0FFFFFFFFFFFFFFF) { n = n + 4; x = x << 4; }
            if (x <= 0x3FFFFFFFFFFFFFFF) { n = n + 2; x = x << 2; }
            if (x <= 0x7FFFFFFFFFFFFFFF) { n = n + 1; }
            return n;
        }
        #endregion

        /// <summary>
        /// Returns a value indicating the sign of a half-precision floating-point number.
        /// </summary>
        /// <param name="value">A signed number.</param>
        /// <returns>
        /// A number indicating the sign of value. Number Description -1 value is less
        /// than zero. 0 value is equal to zero. 1 value is greater than zero.
        /// </returns>
        /// <exception cref="System.ArithmeticException">value is equal to System.Half.NaN.</exception>
        public static int Sign(Half value)
        {
            if (value < 0)
            {
                return -1;
            }
            else if (value > 0)
            {
                return 1;
            }
            else
            {
                if (value != 0)
                {
                    throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
                }
            }

            return 0;
        }
        /// <summary>
        /// Returns the absolute value of a half-precision floating-point number.
        /// </summary>
        /// <param name="value">A number in the range System.Half.MinValue ≤ value ≤ System.Half.MaxValue.</param>
        /// <returns>A half-precision floating-point number, x, such that 0 ≤ x ≤System.Half.MaxValue.</returns>
        public static Half Abs(Half value)
        {
            return Half.ToHalf((ushort)(value.value & 0x7fff));
        }

        /// <summary>
        /// Returns the larger of two half-precision floating-point numbers.
        /// </summary>
        /// <param name="value1">The first of two half-precision floating-point numbers to compare.</param>
        /// <param name="value2">The second of two half-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter value1 or value2, whichever is larger. If value1, or value2, or both val1
        /// and value2 are equal to System.Half.NaN, System.Half.NaN is returned.
        /// </returns>
        public static Half Max(Half value1, Half value2)
        {
            return (value1 < value2) ? value2 : value1;
        }

        /// <summary>
        /// Returns the smaller of two half-precision floating-point numbers.
        /// </summary>
        /// <param name="value1">The first of two half-precision floating-point numbers to compare.</param>
        /// <param name="value2">The second of two half-precision floating-point numbers to compare.</param>
        /// <returns>
        /// Parameter value1 or value2, whichever is smaller. If value1, or value2, or both val1
        /// and value2 are equal to System.Half.NaN, System.Half.NaN is returned.
        /// </returns>
        public static Half Min(Half value1, Half value2)
        {
            return (value1 < value2) ? value1 : value2;
        }
    }
}
