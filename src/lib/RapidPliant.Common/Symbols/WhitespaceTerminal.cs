using System.Collections.Generic;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public class WhitespaceTerminal : BaseTerminal
    {
        private static readonly Interval[] _intervals = {
            new Interval((char)0x0009, (char)0x000D),
            new Interval((char)0x0020, (char)0x0020),
            new Interval((char)0x0085, (char)0x0085),
            new Interval((char)0x00A0, (char)0x00A0),
            new Interval((char)0x2000, (char)0x200A),
            new Interval((char)0x2028, (char)0x2029),
            new Interval((char)0x202f, (char)0x202f),
            new Interval((char)0x205f, (char)0x205f),
            new Interval((char)0x3000, (char)0x3000)
        };

        private const string ToStringValue = @"\s";

        private static readonly int _hashCode = ToStringValue.GetHashCode();

        public WhitespaceTerminal()
        {
        }

        public override bool IsMatch(char character)
        {
            return char.IsWhiteSpace(character);
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

            var whitespaceTerminal = obj as WhitespaceTerminal;
            if (whitespaceTerminal == null)
                return false;

            return true;
        }
        
        public override string ToString()
        {
            return ToStringValue;
        }
    }
}