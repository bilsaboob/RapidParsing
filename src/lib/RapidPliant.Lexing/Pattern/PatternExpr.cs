using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;

namespace RapidPliant.Lexing.Pattern
{
    public abstract class PatternExpr<TExpr> : Expr<TExpr>
        where TExpr : PatternExpr<TExpr>
    {
        public PatternExpr()
            : this(false, false)
        {
        }

        protected PatternExpr(bool isAlteration, bool isProduction)
            : base(isAlteration | isProduction, isAlteration, isProduction)
        {
        }
    }

    public interface IPatternExpr : IExpr
    {
    }

    public interface IPatternTerminalExpr : IPatternExpr
    {
    }

    public interface IPatternTerminalCharExpr : IPatternTerminalExpr
    {
        char Char { get; }
    }

    public interface IPatternCharClassExpr : IPatternTerminalExpr
    {
        string CharClass { get; }
    }

    public interface IPatternTerminalCharRangeExpr : IPatternTerminalExpr
    {
        char FromChar { get; }
        char ToChar { get; }
    }
}
