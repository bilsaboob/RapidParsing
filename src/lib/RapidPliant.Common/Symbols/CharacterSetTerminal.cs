using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public class CharacterSetTerminal : BaseTerminal
    {
        private Interval[] _intervals;

        private int _hashCode;

        public CharacterSetTerminal(CharSet chars)
        {
            _intervals = CreateIntervals(chars);

            ComputeHashCode();
        }

        private Interval[] CreateIntervals(CharSet chars)
        {
            return chars.Ranges.Select(r => new Interval(r.From, r.To)).ToArray();
        }

        public CharSet Chars { get; protected set; }

        public override bool IsMatch(char character)
        {
            return Chars.ContainsChar(character);
        }

        public override IReadOnlyList<Interval> GetIntervals()
        {
            return _intervals;
        }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(_intervals);
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

            var setTerminal = obj as CharacterSetTerminal;
            if (setTerminal == null)
                return false;

            if (_intervals.Length != setTerminal._intervals.Length)
                return false;

            for(var i = 0; i < _intervals.Length; ++i)
            {
                var interval1 = _intervals[i];
                var interval2 = setTerminal._intervals[i];
                if (!interval1.Equals(interval2))
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Chars}";
        }
    }
}
