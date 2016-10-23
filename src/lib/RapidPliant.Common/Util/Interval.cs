using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Common.Util
{
    public class Interval : IComparable<Interval>
    {
        private static readonly Interval[] _empty = { };

        public Interval(char min, char max)
        {
            if (min.CompareTo(max) > 0)
                throw new Exception($"{min} should be less than {max}");

            Min = min;
            Max = max;
        }

        public char Min { get; private set; }

        public char Max { get; private set; }
        
        public bool Overlaps(Interval other)
        {
            return Max.CompareTo(other.Min) >= 0 && Min.CompareTo(other.Max) <= 0;
        }
        
        public bool Touches(Interval other)
        {
            // if intervals overlap they touch
            if (Overlaps(other))
                return true;

            // char.CompareTo returns the difference between integers
            if (Min - other.Max == 1)
                return true;

            if (other.Min - Max == 1)
                return true;

            return false;
        }
        
        public static IReadOnlyList<Interval> Join(Interval first, Interval second)
        {
            var list = new List<Interval>();

            var touches = first.Touches(second);
            if (!touches)
            {
                list.Add(first);
                list.Add(second);
                return list;
            }

            var max = (char)Math.Max(first.Max, second.Max);
            var min = (char)Math.Min(first.Min, second.Min);

            var interval = new Interval(min, max);
            list.Add(interval);

            return list;
        }
        
        public static IReadOnlyList<Interval> Split(Interval first, Interval second)
        {
            var list = new List<Interval>();

            var overlaps = first.Overlaps(second);
            if (!overlaps)
            {
                list.Add(first);
                list.Add(second);
                return list;
            }

            // order matters to ensure intervals are sorted

            var minsAreEqual = first.Min.CompareTo(second.Min) == 0;
            var maxesAreEqual = first.Max.CompareTo(second.Max) == 0;

            if (!minsAreEqual && maxesAreEqual)
            {
                var localMin = (char)Math.Min(first.Min, second.Min);
                var localMax = (char)(Math.Max(first.Min, second.Min) - 1);
                list.Add(new Interval(localMin, localMax));
            }

            var intersectMin = (char)Math.Max(first.Min, second.Min);
            var intersectMax = (char)Math.Min(first.Max, second.Max);
            list.Add(new Interval(intersectMin, intersectMax));

            if (minsAreEqual && !maxesAreEqual)
            {
                var localMin = (char)(Math.Min(first.Max, second.Max) + 1);
                var localMax = (char)Math.Max(first.Max, second.Max);
                list.Add(new Interval(localMin, localMax));
            }

            return list;
        }
        
        public static IReadOnlyList<Interval> Inverse(Interval interval)
        {
            if (interval.Min == char.MinValue && interval.Max == char.MaxValue)
                return _empty;

            var list = new List<Interval>();
            if (interval.Min != char.MinValue)
                list.Add(new Interval(char.MinValue, (char)(interval.Min - 1)));
            if (interval.Max != char.MaxValue)
                list.Add(new Interval((char)(interval.Max + 1), char.MaxValue));

            return list;
        }

        public static IReadOnlyList<Interval> Group(IReadOnlyList<Interval> input)
        {
            var sortedIntervals = new List<Interval>();
            sortedIntervals.AddRange(input);
            sortedIntervals.Sort();

            var intervalList = new List<Interval>();

            Interval accumulator = null;
            for (var i = 0; i < sortedIntervals.Count; i++)
            {
                var interval = sortedIntervals[i];
                if (accumulator == null)
                {
                    accumulator = interval;
                    continue;
                }

                var joins = Join(accumulator, interval);

                switch (joins.Count)
                {
                    case 2:
                    {
                        intervalList.Add(joins[0]);
                        accumulator = joins[1];
                        break;
                    }
                    case 1:
                    {
                        accumulator = joins[0];
                        break;
                    }
                }
            }

            if (accumulator != null)
                intervalList.Add(accumulator);

            return intervalList;
        }

        public int CompareTo(Interval other)
        {
            var compareMin = Min.CompareTo(other.Min);
            if (compareMin != 0)
                return compareMin;
            return Max.CompareTo(other.Max);
        }

        public override string ToString()
        {
            return $"['{Min}', '{Max}']";
        }
    }

    public class SingleCharInterval : Interval
    {
        public SingleCharInterval(char character) 
            : base(character, character)
        {
        }

        public override string ToString()
        {
            return $"'{Min}'";
        }
    }
}
