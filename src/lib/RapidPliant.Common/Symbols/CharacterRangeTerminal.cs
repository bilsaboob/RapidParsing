using System.Collections.Generic;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public class CharacterRangeTerminal : BaseTerminal
    {
        private Interval[] _intervals;

        private int _hashCode;

        public CharacterRangeTerminal(char start, char end)
        {
            Start = start;
            End = end;

            _intervals = new[] {
                new Interval(Start, End)
            };

            ComputeHashCode();
        }
        
        public char Start { get; private set; }

        public char End { get; private set; }

        public override bool IsMatch(char character)
        {
            return Start <= character && character <= End;
        }

        public override IReadOnlyList<Interval> GetIntervals()
        {
            return _intervals;
        }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(Start.GetHashCode(), End.GetHashCode());
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

            var rangeTerminal = obj as CharacterRangeTerminal;
            if (rangeTerminal == null)
                return false;

            return rangeTerminal.End == End && rangeTerminal.Start == Start;
        }
        
        public override string ToString()
        {
            return $"[{Start}-{End}]";
        }
    }
}