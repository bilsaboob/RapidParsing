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
    }

    public interface ILexPatternRule : IRule
    {        
    }

    public interface ILexPatternSymbol : ISymbol
    {
    }

    public interface ILexPatternRuleRefSymbol : ILexPatternSymbol
    {
    }

    public interface ILexPatternTerminalSymbol : ILexPatternSymbol
    {
    }

    public interface ILexPatternTerminalCharSymbol : ILexPatternTerminalSymbol
    {
        char Char { get; }
    }

    public interface ILexPatternTerminalRangeSymbol : ILexPatternTerminalSymbol
    {
        char FromChar { get; }
        char ToChar { get; }
    }
    
}
