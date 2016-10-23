using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;

namespace RapidPliant.Lexing.Pattern
{
    public abstract class PatternExpr<TExpr> : Expr<TExpr>, IPatternExpr
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

    public interface IPatternExpr
    {
    }

    public interface IPatternTerminalExpr
    {
    }

    public interface IPatternTerminalCharExpr : IPatternTerminalExpr
    {
        char Char { get; }
    }

    public interface IPatternTerminalRangeExpr : IPatternTerminalExpr
    {
        char FromChar { get; }

        char ToChar { get; }
    }
}
