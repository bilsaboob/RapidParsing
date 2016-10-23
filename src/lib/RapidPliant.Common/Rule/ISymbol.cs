using System;
using System.CodeDom;
using System.Collections.Generic;
using RapidPliant.Common.Util;

namespace RapidPliant.Common.Rule
{
    public enum SymbolType
    {
        Terminal,

        Repetition,
        Grouping,
        Optional
    }

    public interface ISymbol
    {
        SymbolId SymbolId { get; }
        SymbolType SymbolType { get; }
    }

    public abstract class Symbol : ISymbol
    {
        public Symbol(SymbolType symbolType, SymbolId symbolId)
        {
            SymbolType = symbolType;
            SymbolId = symbolId;
        }

        public SymbolType SymbolType { get; protected set; }

        public SymbolId SymbolId { get; protected set; }
    }
    
    public class SymbolId
    {
        private readonly int _hashCode;

        public SymbolId(Type ownerType, string name)
        {
            OwnerType = ownerType;
            Name = name;

            _hashCode = HashCode.Compute(ownerType.GetHashCode(), name.GetHashCode());
        }

        public Type OwnerType { get; private set; }

        public string Name { get; private set; }

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
            
            var other = obj as SymbolId;
            if (other == null)
                return false;
            
            if (other.OwnerType != OwnerType)
                return false;

            if (other.Name != Name)
                return false;

            return true;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public interface ITerminal : ISymbol
    {
        bool IsMatch(char character);

        IReadOnlyList<Interval> GetIntervals();
    }

    public abstract class BaseTerminal : Symbol, ITerminal
    {
        protected BaseTerminal(SymbolId id) 
            : base(SymbolType.Terminal, id)
        {
        }

        public abstract bool IsMatch(char character);

        public abstract IReadOnlyList<Interval> GetIntervals();
    }

    public class CharacterTerminal : BaseTerminal
    {
        public static readonly SymbolId ID = new SymbolId(typeof(CharacterTerminal), "CharacterTerminal");

        private int _hashCode;

        private Interval[] _intervals;

        public CharacterTerminal(char character)
            : base(ID)
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

    public class CharacterRangeTerminal : BaseTerminal
    {
        public static readonly SymbolId ID = new SymbolId(typeof(CharacterRangeTerminal), "CharacterRangeTerminal");

        private Interval[] _intervals;

        private int _hashCode;

        public CharacterRangeTerminal(char start, char end)
            : base(ID)
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

    public class AnyTerminal : BaseTerminal
    {
        public static readonly SymbolId ID = new SymbolId(typeof(AnyTerminal), "AnyTerminal");

        private static readonly Interval[] _interals = { new Interval(char.MinValue, char.MaxValue) };

        private static int _hashCode = true.GetHashCode();

        public AnyTerminal()
            : base(ID)
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

    public class NegationTerminal : BaseTerminal
    {
        public static readonly SymbolId ID = new SymbolId(typeof(NegationTerminal), "Negation");

        private IReadOnlyList<Interval> _intervals;
        private int _hashCode;

        public NegationTerminal(ITerminal negatedTerminal)
            : base(ID)
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

    public class WhitespaceTerminal : BaseTerminal
    {
        public static readonly SymbolId ID = new SymbolId(typeof(WhitespaceTerminal), "WhitespaceTerminal");

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
            : base(ID)
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

    public class WordTerminal : BaseTerminal
    {
        public static readonly SymbolId ID = new SymbolId(typeof(WordTerminal), "WordTerminal");

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
            : base(ID)
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

    public class DigitTerminal : BaseTerminal
    {
        public static readonly SymbolId ID = new SymbolId(typeof(DigitTerminal), "DigitTerminal");

        private static readonly Interval[] _intervals = { new Interval('0', '9') };

        private const string ToStringValue = "[0-9]";

        private static readonly int _hashCode = ToStringValue.GetHashCode();

        public DigitTerminal()
            : base(ID)
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