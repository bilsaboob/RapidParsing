using System.Collections.Generic;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public class NegationTerminal : BaseTerminal
    {
        private IReadOnlyList<Interval> _intervals;
        private int _hashCode;

        public NegationTerminal(ITerminal negatedTerminal)
        {
            NegatedTerminal = negatedTerminal;
            ComputeHashCode();
        }
        
        public ITerminal NegatedTerminal { get; private set; }

        public override bool IsMatch(char character)
        {
            return !NegatedTerminal.IsMatch(character);
        }

        public override IReadOnlyList<Interval> GetIntervals()
        {
            if (_intervals == null)
                _intervals = CreateIntervals(NegatedTerminal);

            return _intervals;
        }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(false.GetHashCode(), NegatedTerminal.GetHashCode());
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj == this)
                return true;

            var negationTerminal = obj as NegationTerminal;
            if (negationTerminal == null)
                return false;

            return NegatedTerminal.Equals(negationTerminal);
        }

        private static IReadOnlyList<Interval> CreateIntervals(ITerminal innerTerminal)
        {
            var inverseIntervalList = new List<Interval>();
            var intervals = innerTerminal.GetIntervals();
            for (var i = 0; i < intervals.Count; i++)
            {
                var inverseIntervals = Interval.Inverse(intervals[i]);
                inverseIntervalList.AddRange(inverseIntervals);
            }

            return Interval.Group(inverseIntervalList);
        }
    }
}