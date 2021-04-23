using System;

namespace Krauler
{
    internal static class Comparators
    {
        public const double Epsilon = 1e-6;

        /// <summary>
        /// Performans a correct floating point compare approximation
        /// using epsilon and ulp.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static unsafe bool NearEqual(float x, float y)
        {
            // Check if they are really close with epsilon
            if (Math.Abs(x - y) < Epsilon)
            {
                return true;
            }

            // *reinterpret_cast<std::int32_t*>(&x)
            int a = *(int*)&x;
            int b = *(int*)&y;

            // Compare sign
            if (a < 0 != b < 0)
                return false;

            // Units in last place
            int ulp = Math.Abs(a - b);
            return ulp <= 4;
        }

        /// <summary>
        /// Performans a correct floating point compare approximation
        /// using epsilon and ulp.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static unsafe bool NearEqual(double x, double y)
        {
            // Check if they are really close with epsilon
            if (Math.Abs(x - y) < Epsilon)
            {
                return true;
            }

            // *reinterpret_cast<std::int64_t*>(&x)
            long a = *(long*)&x;
            long b = *(long*)&y;

            // Compare sign
            if (a < 0 != b < 0)
                return false;

            // Units in last place
            long ulp = Math.Abs(a - b);
            return ulp <= 4;
        }

        /// <summary>
        /// Checks if a - b are almost equals within a float epsilon.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool WithinEpsilon(float a, float b, float epsilon)
        {
            float num = a - b;
            return -epsilon <= num && (num <= epsilon);
        }
    }
}
