using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Common.Util;

namespace RapidPliant.Lexing.Pattern
{
    public class LexPatternRule : PatternRule<LexPatternRule>, ILexPatternRule
    {
        public LexPatternRule()
            : this(null)
        {
        }

        public LexPatternRule(string name)
            : base(name)
        {
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return Name;

            return "L";
        }
    }

    public interface ILexPatternRule : IRule
    {        
    }

    public interface ILexPatternSymbol : ISymbol
    {
    }

    public abstract class LexPatternSymbol : Symbol, ILexPatternSymbol
    {
        public LexPatternSymbol(SymbolType symbolType)
            : base(symbolType)
        {
        }
    }

    public interface ILexPatternRuleRefSymbol : ILexPatternSymbol
    {
        IRule Rule { get; }
    }

    public class LexPatternRuleRefSymbol : LexPatternSymbol, ILexPatternRuleRefSymbol
    {
        public static readonly SymbolType Type = new SymbolType(typeof(LexPatternRuleRefSymbol), "RuleRef");

        private int _hashCode;

        public LexPatternRuleRefSymbol(IRule rule)
            : base(Type)
        {
            Rule = rule;

            ComputeHashCode();
        }

        public IRule Rule { get; private set; }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(SymbolType.GetHashCode(), Rule.GetHashCode());
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

            var other = obj as LexPatternRuleRefSymbol;
            if (other == null)
                return false;

            if (!SymbolType.Equals(other.SymbolType))
                return false;

            if (Rule != null)
            {
                if (!Rule.Equals(other.Rule))
                    return false;
            }
            else
            {
                if (other.Rule != null)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Format("ref:" + Rule.Name);
        }
    }

    public interface ILexPatternTerminalSymbol : ILexPatternSymbol
    {
    }

    public abstract class LexPatternTerminalSymbol : LexPatternSymbol, ILexPatternTerminalSymbol
    {
        public LexPatternTerminalSymbol(SymbolType symbolType) : base(symbolType)
        {
        }
    }

    public interface ILexPatternTerminalCharSymbol : ILexPatternTerminalSymbol
    {
        char Char { get; }
    }

    public class LexPatternTerminalCharSymbol : LexPatternTerminalSymbol, ILexPatternTerminalCharSymbol
    {
        public static readonly SymbolType Type = new SymbolType(typeof(LexPatternTerminalCharSymbol), "TerminalChar");

        private int _hashCode;

        public LexPatternTerminalCharSymbol(char character)
            : base(Type)
        {
            Char = character;

            ComputeHashCode();
        }
        
        public char Char { get; private set; }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(SymbolType.GetHashCode(), Char.GetHashCode());
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

            var other = obj as LexPatternTerminalCharSymbol;
            if (other == null)
                return false;

            if (!SymbolType.Equals(other.SymbolType))
                return false;

            if (other.Char != Char)
                return false;

            return true;
        }

        public override string ToString()
        {
            return Char.ToString();
        }
    }

    public interface ILexPatternTerminalRangeSymbol : ILexPatternTerminalSymbol
    {
        char FromChar { get; }
        char ToChar { get; }
    }

    public class LexPatternTerminalRangeSymbol : LexPatternTerminalSymbol, ILexPatternTerminalRangeSymbol
    {
        public static readonly SymbolType Type = new SymbolType(typeof(LexPatternTerminalRangeSymbol), "TerminalRange");

        private int _hashCode;

        public LexPatternTerminalRangeSymbol(char fromChar, char toChar)
            : base(Type)
        {
            FromChar = fromChar;
            ToChar = toChar;

            ComputeHashCode();
        }
        
        public char FromChar { get; private set; }
        public char ToChar { get; private set; }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(SymbolType.GetHashCode(), FromChar.GetHashCode(), ToChar.GetHashCode());
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

            var other = obj as LexPatternTerminalRangeSymbol;
            if (other == null)
                return false;

            if (!SymbolType.Equals(other.SymbolType))
                return false;

            if (other.FromChar != FromChar)
                return false;

            if (other.ToChar != ToChar)
                return false;

            return true;
        }

        public override string ToString()
        {
            return string.Format("({0}-{1})", FromChar, ToChar);
        }
    }


}
