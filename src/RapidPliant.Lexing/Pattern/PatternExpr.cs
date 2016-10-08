using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;

namespace RapidPliant.Lexing.Pattern
{
    public interface IPatternExpr : IExpr
    {
    }

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
}
