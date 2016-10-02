using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar.Definitions
{
    public abstract class RuleDef : GrammarDef, IRuleDef
    {
        public RuleDef()
        {
        }

        public IExpr Expression { get; private set; }

        public void As(Expr expr)
        {
            Expression = expr;
        }

        /*
public override string ToString()
{
    var sb = new StringBuilder();
    if (_defExpr != null)
    {
        _defExpr.ToString(sb);
    }
    else
    {
        ToString(sb);
    }
    return sb.ToString();
}

public override void ToString(StringBuilder sb)
{
    if (string.IsNullOrEmpty(Name))
    {
        sb.Append("?UNKNOWN?");
    }
    else
    {
        sb.Append(Name);
    }
}
*/
    }

    public partial class Rule : RuleDef
    {
        public Rule()
        {
        }
    }

    public partial class Rule : RuleDef
    {
        public static implicit operator Rule(string ruleName)
        {
            return new Rule() {
                Name = ruleName
            };
        }

        public static implicit operator GrammarExpr(Rule rule)
        {
            return GrammarDef.RuleRef(rule);
        }

        #region And
        public static GrammarExpr operator +(GrammarExpr lhs, Rule rhs)
        {
            return GrammarExpr.AddWithAnd(lhs, GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator +(Rule lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.RuleRef(lhs), rhs);
        }
        public static GrammarExpr operator +(Rule lhs, Rule rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.RuleRef(lhs), GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator +(Rule lhs, char rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.RuleRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator +(char lhs, Rule rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator +(Rule lhs, string rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.RuleRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }

        public static GrammarExpr operator +(string lhs, Rule rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), GrammarDef.RuleRef(rhs));
        }
        #endregion

        #region Or
        public static GrammarExpr operator |(GrammarExpr lhs, Rule rhs)
        {
            return GrammarExpr.AddWithOr(lhs, GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator |(Rule lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.RuleRef(lhs), rhs);
        }
        public static GrammarExpr operator |(Rule lhs, Rule rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.RuleRef(lhs), GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator |(Rule lhs, char rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.RuleRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(char lhs, Rule rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator |(Rule lhs, string rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.RuleRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(string lhs, Rule rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), GrammarDef.RuleRef(rhs));
        }
        #endregion
    }
    
}
