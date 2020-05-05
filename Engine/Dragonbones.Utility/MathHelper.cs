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
        /// <param name="divisor">the number to be divided by (must be a power of two)</param>
        /// <param name="shiftCount">the power of two the divisor is (or number of shifts left from 1)</param>
        /// <param name="remainder">the returned remainder from division</param>
        /// <returns>the result of the division</returns>
        public static int MathShiftRem(int dividend, int divisor, int shiftCount, out int remainder)
        {
            remainder = dividend & (divisor - 1);
            return dividend >> shiftCount;
        }

        /// <summary>
        /// Faster way to do a value conditional for ints
        /// </summary>
        /// <param name="trueVal">the value to return on true</param>
        /// <param name="falseVal">the value to return on false</param>
        /// <param name="condition">the conditional</param>
        /// <returns></returns>
        public static int FastConditional(int trueVal, int falseVal, bool condition)
        {
            int iCondition = NativeMath.ToInt(condition);
            return trueVal * iCondition + falseVal * (1 - iCondition);
        }

        /// <summary>
        /// Faster way to do a value conditional for ints
        /// </summary>
        /// <param name="trueVal">the value to return on true</param>
        /// <param name="falseVal">the value to return on false</param>
        /// <param name="condition">the conditional</param>
        /// <returns></returns>
        public static float FastConditional(float trueVal, float falseVal, bool condition)
        {
            float iCondition = NativeMath.ToFloat(condition);
            return trueVal * iCondition + falseVal * (1 - iCondition);
        }

        /// <summary>
        /// Faster way to do a value conditional for ints
        /// </summary>
        /// <param name="trueVal">the value to return on true</param>
        /// <param name="falseVal">the value to return on false</param>
        /// <param name="condition">the conditional</param>
        /// <returns></returns>
        public static int FastConditional(int trueVal, int falseVal, int condition)
        {
            return trueVal * condition + falseVal * (1 - condition);
        }

        /// <summary>
        /// Faster way to do a value conditional for ints
        /// </summary>
        /// <param name="trueVal">the value to return on true</param>
        /// <param name="falseVal">the value to return on false</param>
        /// <param name="condition">the conditional</param>
        /// <returns></returns>
        public static float FastConditional(float trueVal, float falseVal, float condition)
        {
            return trueVal * condition + falseVal * (1 - condition);
        }
    }
}
