using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
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
    }
}
