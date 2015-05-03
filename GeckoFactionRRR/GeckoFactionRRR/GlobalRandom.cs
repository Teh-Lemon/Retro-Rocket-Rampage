// Created by Anthony Lee on 03/05/2014
// Random number generator
// Makes it so there can only be one instance of random throughout the game

using System;

namespace GeckoFactionRRR
{
    class GlobalRandom : Random
    {
        // The whole game uses one instance of Random
        static Random instance;

        public static Random Instance
        {
            get
            {
                // Only create one instance of Random
                if (instance == null)
                {
                    instance = new Random();
                }

                return instance;
            }
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>A 32-bit signed integer greater than or equal to zero and less than System.Int32.MaxValue.</returns>
        public static int Next()
        {
            return Instance.Next();
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        /// <para>The exclusive upper bound of the random number to be generated. maxValue</para>
        /// <para>must be greater than or equal to zero.</para>
        /// </param>
        /// <returns>
        /// <para>A 32-bit signed integer greater than or equal to zero, and less than maxValue;</para>
        /// <para>that is, the range of return values ordinarily includes zero but not maxValue.</para>
        /// <para>However, if maxValue equals zero, maxValue is returned.</para>
        /// </returns>
        public static int Next(int maxValue)
        {
            return Instance.Next(maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">
        /// The inclusive lower bound of the random number returned.
        /// </param>
        /// <param name="maxValue">
        /// <para>The exclusive upper bound of the random number returned. maxValue must be</para>
        /// <para>greater than or equal to minValue.</para>
        /// </param>
        /// <returns>
        /// </returns>
        public static int Next(int minValue, int maxValue)
        {
            return Instance.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0
        /// </summary>
        /// <returns>
        /// <para>A double-precision floating point number greater than or equal to 0.0, and</para>
        /// <para>less than 1.0.</para>
        /// </returns>
        public static double NextDouble()
        {
            return Instance.NextDouble();
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes to contain random numbers.
        /// </param>
        public static void NextBytes(byte[] buffer)
        {
            Instance.NextBytes(buffer);
        }

        // Hidden contructor, don't make an instance of GlobalRandom
        private GlobalRandom()
        {
        }
    }
}
