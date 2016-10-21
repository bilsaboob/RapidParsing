using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Common.Util
{
    public static class HashCode
    {
        private const uint SEED = 2166136261;
        private const int INCREMENTAL = 16777619;

        public static int Compute(int first)
        {
            unchecked
            {
                var hash = (int)SEED;
                hash = hash * INCREMENTAL ^ first;
                return hash;
            }
        }

        public static int Compute(int first, int second)
        {
            unchecked
            {
                var hash = (int)SEED;
                hash = hash * INCREMENTAL ^ first;
                hash = hash * INCREMENTAL ^ second;
                return hash;
            }
        }

        public static int Compute(int first, int second, int third)
        {
            unchecked
            {
                var hash = (int)SEED;
                hash = hash * INCREMENTAL ^ first;
                hash = hash * INCREMENTAL ^ second;
                hash = hash * INCREMENTAL ^ third;
                return hash;
            }
        }

        public static int Compute(int first, int second, int third, int fourth)
        {
            unchecked
            {
                var hash = (int)SEED;
                hash = hash * INCREMENTAL ^ first;
                hash = hash * INCREMENTAL ^ second;
                hash = hash * INCREMENTAL ^ third;
                hash = hash * INCREMENTAL ^ fourth;
                return hash;
            }
        }

        public static int Compute(int first, int second, int third, int fourth, int fifth)
        {
            unchecked
            {
                var hash = (int)SEED;
                hash = hash * INCREMENTAL ^ first;
                hash = hash * INCREMENTAL ^ second;
                hash = hash * INCREMENTAL ^ third;
                hash = hash * INCREMENTAL ^ fourth;
                hash = hash * INCREMENTAL ^ fifth;
                return hash;
            }
        }

        public static int Compute(int first, int second, int third, int fourth, int fifth, int sixth)
        {
            unchecked
            {
                var hash = (int)SEED;
                hash = hash * INCREMENTAL ^ first;
                hash = hash * INCREMENTAL ^ second;
                hash = hash * INCREMENTAL ^ third;
                hash = hash * INCREMENTAL ^ fourth;
                hash = hash * INCREMENTAL ^ fifth;
                hash = hash * INCREMENTAL ^ sixth;
                return hash;
            }
        }

        public static int Compute(IEnumerable<object> items)
        {
            unchecked
            {
                var hash = (int)SEED;
                foreach (var item in items)
                {
                    hash = hash * INCREMENTAL ^ item.GetHashCode();
                }
                return hash;
            }
        }

        public static int ComputeIncrementalHash(int hashCode, int accumulator, bool isFirstValue = false)
        {
            unchecked
            {
                if (isFirstValue)
                {
                    accumulator = (int)SEED;
                }
                accumulator = accumulator * INCREMENTAL ^ hashCode;
                return accumulator;
            }
        }

        public static bool EqualsArray<T>(this T[] array, T[] otherArray)
        {
            if (array == otherArray)
                return true;

            if (array == null && otherArray == null)
                return true;

            if (array != null && otherArray != null)
            {
                var len1 = array.Length;
                var len2 = otherArray.Length;

                if (len1 != len2)
                    return false;

                for (var i = 0; i < len1; ++i)
                {
                    var elem1 = array[i];
                    var elem2 = otherArray[i];

                    if (elem1 == null && elem2 != null)
                        return false;

                    if (elem2 == null && elem1 != null)
                        return false;

                    if (!elem1.Equals(elem2))
                        return false;
                }

                return true;
            }

            return false;
        }
    }
}
