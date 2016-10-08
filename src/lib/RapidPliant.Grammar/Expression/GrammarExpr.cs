using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Grammar.Definitions;

namespace RapidPliant.Grammar.Expression
{
    public partial class GrammarExpr : Expr<GrammarExpr>
    {
        public GrammarExpr()
            : this(false, false)
        {
        }

        protected GrammarExpr(bool isAlteration, bool isProduction) 
            : base(isAlteration | isProduction, isAlteration, isProduction)
        {
        }

        protected override GrammarExpr CreateAlterationExpr()
        {
            return new GrammarExpr(true, false);
        }

        protected override GrammarExpr CreateProductionExpr()
        {
            return new GrammarExpr(false, true);
        }
    }

    public partial class GrammarExpr : Expr<GrammarExpr>
    {
        #region And
        public static GrammarExpr operator +(GrammarExpr lhs, GrammarExpr rhs)
        {
            return AddWithAnd(lhs, rhs);
        }
        public static GrammarExpr operator +(GrammarExpr lhs, char rhs)
        {
            return AddWithAnd(lhs, InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator +(char lhs, GrammarExpr rhs)
        {
            return AddWithAnd(InPlaceLexRef(lhs), rhs);
        }
        public static GrammarExpr operator +(GrammarExpr lhs, string rhs)
        {
            return AddWithAnd(lhs, InPlaceLexRef(rhs));
        }

        public static GrammarExpr operator +(string lhs, GrammarExpr rhs)
        {
            return AddWithAnd(InPlaceLexRef(lhs), rhs);
        }
        #endregion

        #region Or
        public static GrammarExpr operator |(GrammarExpr lhs, GrammarExpr rhs)
        {
            return AddWithOr(lhs, rhs);
        }
        public static GrammarExpr operator |(GrammarExpr lhs, char rhs)
        {
            return AddWithOr(lhs, InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(char lhs, GrammarExpr rhs)
        {
            return AddWithOr(InPlaceLexRef(lhs), rhs);
        }
        public static GrammarExpr operator |(GrammarExpr lhs, string rhs)
        {
            return AddWithOr(lhs, InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(string lhs, GrammarExpr rhs)
        {
            return AddWithOr(InPlaceLexRef(lhs), rhs);
        }
        #endregion

        #region helpers
        public static RuleRefExpr RuleRef(RuleDef ruleDef)
        {
            return GrammarDef.RuleRef(ruleDef);
        }

        public static LexRefExpr LexRef(LexDef lexDef)
        {
            return GrammarDef.LexRef(lexDef);
        }

        public static ILexModel LexModelForString(string str)
        {
            return GrammarDef.LexModelForString(str);
        }

        public static LexRefExpr InPlaceLexRef(string spelling)
        {
            return GrammarDef.InPlaceLexRef(spelling);
        }

        public static LexRefExpr InPlaceLexRef(char character)
        {
            return GrammarDef.InPlaceLexRef(character);
        }

        public static InPlaceLexDef InPlaceLexDef(string spelling)
        {
            return GrammarDef.InPlaceLexDef(spelling);
        }

        public static InPlaceLexDef InPlaceLexDef(char character)
        {
            return GrammarDef.InPlaceLexDef(character);
        }
        #endregion
    }

    public interface IRuleRefExpr : IExpr
    {
        IRuleDef RuleDef { get; }
    }

    public class RuleRefExpr : GrammarExpr, IRuleRefExpr
    {
        public RuleRefExpr(IRuleDef ruleDef)
        {
            RuleDef = ruleDef;
        }

        public IRuleDef RuleDef { get; private set; }
    }

    public interface ILexRefExpr : IExpr
    {
        ILexDef LexDef { get; }
    }

    public class LexRefExpr : GrammarExpr, ILexRefExpr
    {
        public LexRefExpr(ILexDef lexDef)
        {
            LexDef = lexDef;
        }

        public ILexDef LexDef { get; private set; }
    }
}
