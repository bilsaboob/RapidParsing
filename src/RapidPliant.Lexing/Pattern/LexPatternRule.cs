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
            Name = name;
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
        public static readonly SymbolType _SymbolType = new SymbolType(typeof(LexPatternRuleRefSymbol), "RuleRef");

        public LexPatternRuleRefSymbol(IRule rule)
            : base(_SymbolType)
        {
            Rule = rule;
        }

        public IRule Rule { get; private set; }
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
        public static readonly SymbolType _SymbolType = new SymbolType(typeof(LexPatternTerminalCharSymbol), "TerminalChar");

        private readonly int _hashCode;

        public LexPatternTerminalCharSymbol(char character)
            : base(_SymbolType)
        {
            Char = character;

            _hashCode = HashCode.Compute(SymbolType.GetHashCode(), Char.GetHashCode());
        }

        public char Char { get; private set; }

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

            if (other.SymbolType != SymbolType)
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
        public static readonly SymbolType _SymbolType = new SymbolType(typeof(LexPatternTerminalRangeSymbol), "TerminalRange");

        private readonly int _hashCode;

        public LexPatternTerminalRangeSymbol(char fromChar, char toChar)
            : base(_SymbolType)
        {
            FromChar = fromChar;
            ToChar = toChar;

            _hashCode = HashCode.Compute(SymbolType.GetHashCode(), FromChar.GetHashCode(), ToChar.GetHashCode());
        }

        public char FromChar { get; private set; }
        public char ToChar { get; private set; }

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

            if (other.SymbolType != SymbolType)
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
