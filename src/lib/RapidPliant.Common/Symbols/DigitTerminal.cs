using System.Collections.Generic;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public class DigitTerminal : BaseTerminal
    {
        private static readonly Interval[] _intervals = { new Interval('0', '9') };

        private const string ToStringValue = "[0-9]";

        private static readonly int _hashCode = ToStringValue.GetHashCode();

        public DigitTerminal()
        {
        }

        public override bool IsMatch(char character)
        {
            return char.IsDigit(character);
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

            var digitTerminal = obj as DigitTerminal;
            if (digitTerminal == null)
                return false;

            return true;
        }

        public override string ToString()
        {
            return ToStringValue;
        }
    }
}