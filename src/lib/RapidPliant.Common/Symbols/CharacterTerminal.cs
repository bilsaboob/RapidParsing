using System.Collections.Generic;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public class CharacterTerminal : BaseTerminal
    {
        private int _hashCode;

        private Interval[] _intervals;

        public CharacterTerminal(char character)
        {
            Character = character;

            _intervals = new[] { new SingleCharInterval(Character) };

            ComputeHashCode();
        }
        
        public char Character { get; private set; }

        public override bool IsMatch(char character)
        {
            return Character == character;
        }

        public override IReadOnlyList<Interval> GetIntervals()
        {
            return _intervals;
        }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(Character.GetHashCode());
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

            var terminal = obj as CharacterTerminal;
            if (terminal == null)
                return false;

            return terminal.Character == Character;
        }
        
        public override string ToString()
        {
            return $"'{Character}'";
        }
    }
}