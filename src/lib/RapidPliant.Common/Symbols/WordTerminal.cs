using System.Collections.Generic;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public class WordTerminal : BaseTerminal
    {
        private static readonly Interval[] _intervals =
        {
            new Interval('A', 'Z'),
            new Interval('a', 'z'),
            new Interval('0', '9'),
            new Interval('_', '_')
        };

        private const string ToStringValue = @"\s";

        private static readonly int _hashCode = ToStringValue.GetHashCode();

        public WordTerminal()
        {
        }

        public override bool IsMatch(char character)
        {
            return ('A' <= character && character <= 'Z'
                    || 'a' <= character && character <= 'z'
                    || '0' <= character && character <= '9'
                    || '_' == character);
        }

        public override IReadOnlyList<Interval> GetIntervals()
        {
            return _intervals;
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

            var wordTerminal = obj as WordTerminal;
            if (wordTerminal == null)
                return false;

            return true;
        }
        
        public override string ToString()
        {
            return ToStringValue;
        }
    }
}