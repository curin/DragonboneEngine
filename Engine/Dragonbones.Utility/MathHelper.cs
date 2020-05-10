using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using Dragonbones.Native;

namespace Dragonbones
{
    /// <summary>
    /// A helper class filled with some useful math functions
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Calculates an average for a stream of data
        /// </summary>
        /// <param name="currentAverage">the current average</param>
        /// <param name="addedValue">the new value to add to the set</param>
        /// <param name="numberCount">the total number of values including the new one</param>
        /// <returns>the new average</returns>
        public static int MovingAverage(int currentAverage, int addedValue, int numberCount)
        {
            return currentAverage + ((addedValue - currentAverage) / numberCount);
        }

        /// <summary>
        /// Calculates an average for a stream of data
        /// </summary>
        /// <param name="currentAverage">the current average</param>
        /// <param name="addedValue">the new value to add to the set</param>
        /// <param name="numberCount">the total number of values including the new one</param>
        /// <returns>the new average</returns>
        public static float MovingAverage(float currentAverage, float addedValue, long numberCount)
        {
            return currentAverage + ((addedValue - currentAverage) / numberCount);
        }

        /// <summary>
        /// Calculates an average for a stream of data
        /// </summary>
        /// <param name="currentAverage">the current average</param>
        /// <param name="addedValue">the new value to add to the set</param>
        /// <param name="numberCount">the total number of values including the new one</param>
        /// <returns>the new average</returns>
        public static long MovingAverage(long currentAverage, long addedValue, long numberCount)
        {
            return currentAverage + ((addedValue - currentAverage) / numberCount);
        }

        /// <summary>
        /// Calculates an average for a stream of data
        /// </summary>
        /// <param name="currentAverage">the current average</param>
        /// <param name="addedValue">the new value to add to the set</param>
        /// <param name="numberCount">the total number of values including the new one</param>
        /// <returns>the new average</returns>
        public static double MovingAverage(double currentAverage, double addedValue, long numberCount)
        {
            return currentAverage + ((addedValue - currentAverage) / numberCount);
        }

        /// <summary>
        /// Fast Division and remainder if divisor is a power of two
        /// </summary>
        /// <param name="dividend">the number to be divided</param>
        /// <param name="divisorMinusOne">the number to be divided by (must be a power of two) minus 1</param>
        /// <param name="shiftCount">the power of two the divisor is (or number of shifts left from 1)</param>
        /// <param name="remainder">the returned remainder from division</param>
        /// <returns>the result of the division</returns>
        public static int MathShiftRem(int dividend, int divisorMinusOne, int shiftCount, out int remainder)
        {
            remainder = dividend & (divisorMinusOne);
            return dividend >> shiftCount;
        }

        /// <summary>
        /// Fast Division and remainder if divisor is a power of two
        /// </summary>
        /// <param name="dividend">the number to be divided</param>
        /// <param name="divisorMinusOne">the number to be divided by (must be a power of two) minus 1</param>
        /// <param name="shiftCount">the power of two the divisor is (or number of shifts left from 1)</param>
        /// <param name="remainder">the returned remainder from division</param>
        /// <returns>the result of the division</returns>
        public static long MathShiftRem(long dividend, long divisorMinusOne, int shiftCount, out long remainder)
        {
            remainder = dividend & (divisorMinusOne);
            return dividend >> shiftCount;
        }

        /// <summary>
        /// Fast Division and remainder if divisor is a power of two
        /// </summary>
        /// <param name="dividend">the number to be divided</param>
        /// <param name="divisorMinusOne">the number to be divided by (must be a power of two) minus 1</param>
        /// <param name="shiftCount">the power of two the divisor is (or number of shifts left from 1)</param>
        /// <param name="remainder">the returned remainder from division</param>
        /// <returns>the result of the division</returns>
        public static int MathShiftRem(long dividend, long divisorMinusOne, int shiftCount, out int remainder)
        {
            remainder = (int)(dividend & (divisorMinusOne));
            return (int)(dividend >> shiftCount);
        }

        /// <summary>
        /// A form of modulus that ensures that the result is positive
        /// </summary>
        /// <param name="dividend">the number to be divided</param>
        /// <param name="divisor">the number to be divided by</param>
        /// <returns></returns>
        public static int MathMod(int dividend, int divisor)
        {
            return ((dividend % divisor) + divisor) % divisor;
        }

        private const ulong DeBruijnSequence = 0x37E84A99DAE458F;

        private static readonly int[] MultiplyDeBruijnBitPosition =
        {
        0, 1, 17, 2, 18, 50, 3, 57,
        47, 19, 22, 51, 29, 4, 33, 58,
        15, 48, 20, 27, 25, 23, 52, 41,
        54, 30, 38, 5, 43, 34, 59, 8,
        63, 16, 49, 56, 46, 21, 28, 32,
        14, 26, 24, 40, 53, 37, 42, 7,
        62, 55, 45, 31, 13, 39, 36, 6,
        61, 44, 12, 35, 60, 11, 10, 9,
    };

        /// <summary>
        /// Search the mask data from least significant bit (LSB) to the most significant bit (MSB) for a set bit (1)
        /// using De Bruijn sequence approach. Warning: Will return zero for b = 0.
        /// </summary>
        /// <param name="b">Target number.</param>
        /// <returns>Zero-based position of LSB (from right to left).</returns>
        public static int BitScanForward(long b)
        {
            return MultiplyDeBruijnBitPosition[((ulong)(b & -b) * DeBruijnSequence) >> 58];
        }

        private static int[] deBruijnTable = {
         0,  0,  0,  1,  0, 16,  2,  0, 29,  0, 17,  0,  0,  3,  0, 22,
        30,  0,  0, 20, 18,  0, 11,  0, 13,  0,  0,  4,  0,  7,  0, 23,
        31,  0, 15,  0, 28,  0,  0, 21,  0, 19,  0, 10, 12,  0,  6,  0,
         0, 14, 27,  0,  0,  9,  0,  5,  0, 26,  8,  0, 25,  0, 24,  0,
        };

        private const uint DeBruijnSequenceInt = 0x06EB14F9;
        /// <summary>
        /// Search the mask data from least significant bit (LSB) to the most significant bit (MSB) for a set bit (1)
        /// using De Bruijn sequence approach. Warning: Will return zero for b = 0.
        /// </summary>
        /// <param name="b">Target number.</param>
        /// <returns>Zero-based position of LSB (from right to left).</returns>
        public static int BitScanForward(int b)
        {
            return deBruijnTable[((uint)(b & -b) * DeBruijnSequenceInt) >> 26];
        }
    }
}
