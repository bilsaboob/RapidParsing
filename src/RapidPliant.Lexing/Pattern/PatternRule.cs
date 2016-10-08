using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;

namespace RapidPliant.Lexing.Pattern
{
    public class PatternRule<TRule> : Rule<TRule>
        where TRule : PatternRule<TRule>, new()
    {
        public void FromExpression(IPatternExpr expr)
        {
        }
    }
}
