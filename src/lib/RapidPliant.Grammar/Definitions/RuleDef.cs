using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar.Definitions
{
    public partial class RuleDef : GrammarDef, IRuleDef
    {
        public RuleDef()
        {
        }

        public GrammarExpr Expression { get; private set; }
        IExpr IRuleDef.Expression { get { return Expression; } }

        public RuleDef As(GrammarExpr expr)
        {
            Expression = expr;
            return this;
        }
        
        public override string ToString()
        {
            return $"R:{Name}";
        }
    }

    public partial class RuleDef
    {
        public static implicit operator RuleDef(string ruleName)
        {
            return new RuleDef() {
                Name = ruleName
            };
        }

        public static implicit operator GrammarExpr(RuleDef RuleDef)
        {
            return GrammarDef.RuleRef(RuleDef);
        }

        #region And
        public static GrammarExpr operator +(GrammarExpr lhs, RuleDef rhs)
        {
            return GrammarExpr.AddWithAnd(lhs, GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator +(RuleDef lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.RuleRef(lhs), rhs);
        }
        public static GrammarExpr operator +(RuleDef lhs, RuleDef rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.RuleRef(lhs), GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator +(RuleDef lhs, char rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.RuleRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator +(char lhs, RuleDef rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator +(RuleDef lhs, string rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.RuleRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }

        public static GrammarExpr operator +(string lhs, RuleDef rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), GrammarDef.RuleRef(rhs));
        }
        #endregion

        #region Or
        public static GrammarExpr operator |(GrammarExpr lhs, RuleDef rhs)
        {
            return GrammarExpr.AddWithOr(lhs, GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator |(RuleDef lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.RuleRef(lhs), rhs);
        }
        public static GrammarExpr operator |(RuleDef lhs, RuleDef rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.RuleRef(lhs), GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator |(RuleDef lhs, char rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.RuleRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(char lhs, RuleDef rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), GrammarDef.RuleRef(rhs));
        }
        public static GrammarExpr operator |(RuleDef lhs, string rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.RuleRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(string lhs, RuleDef rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), GrammarDef.RuleRef(rhs));
        }
        #endregion
    }
    
}
