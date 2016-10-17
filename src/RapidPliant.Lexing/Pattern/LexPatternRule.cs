using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;

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
    }

    public interface ILexPatternRuleRefSymbol : ILexPatternSymbol
    {
        IRule Rule { get; }
    }

    public class LexPatternRuleRefSymbol : LexPatternSymbol, ILexPatternRuleRefSymbol
    {
        public LexPatternRuleRefSymbol(IRule rule)
        {
            Rule = rule;
        }

        public IRule Rule { get; private set; }
    }

    public interface ILexPatternTerminalSymbol : ILexPatternSymbol
    {
    }

    public class LexPatternTerminalSymbol : LexPatternSymbol, ILexPatternTerminalSymbol
    {
    }

    public interface ILexPatternTerminalCharSymbol : ILexPatternTerminalSymbol
    {
        char Char { get; }
    }

    public class LexPatternTerminalCharSymbol : LexPatternSymbol, ILexPatternTerminalCharSymbol
    {
        public LexPatternTerminalCharSymbol(char character)
        {
            Char = character;
        }

        public char Char { get; private set; }

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

    public class LexPatternTerminalRangeSymbol : LexPatternSymbol, ILexPatternTerminalRangeSymbol
    {
        public LexPatternTerminalRangeSymbol(char fromChar, char toChar)
        {
            FromChar = fromChar;
            ToChar = toChar;
        }

        public char FromChar { get; private set; }
        public char ToChar { get; private set; }

        public override string ToString()
        {
            return string.Format("({0}-{1})", FromChar, ToChar);
        }
    }


}
