using System.Collections.Generic;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public class AnyTerminal : BaseTerminal
    {
        // full UTF
        //private static readonly Interval[] _interals = { new Interval(char.MinValue, char.MaxValue) };

        private static readonly Interval[] _interals = { new Interval((char)0, (char)256) };

        private static int _hashCode = true.GetHashCode();

        public AnyTerminal()
        {
        }

        public override bool IsMatch(char character)
        {
            return true;
        }

        public override IReadOnlyList<Interval> GetIntervals()
        {
            return _interals;
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

            var anyTerminal = obj as AnyTerminal;
            if (anyTerminal == null)
                return false;

            return true;
        }

        public override string ToString()
        {
            return ".";
        }
    }
}