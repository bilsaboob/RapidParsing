using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public partial class GrammarExpr : Expr
    {
        private static GrammarExprFactory ExprFactory = new GrammarExprFactory();

        public static GrammarExpr AddWithOr(Expr lhsExpr, Expr rhsExpr)
        {
            return (GrammarExpr)Expr.AddWithOr(lhsExpr, rhsExpr, ExprFactory);
        }
        
        public static GrammarExpr AddWithAnd(Expr lhsExpr, Expr rhsExpr)
        {
            return (GrammarExpr)Expr.AddWithAnd(lhsExpr, rhsExpr, ExprFactory);
        }
    }

    public partial class GrammarExpr : Expr
    {
        #region And
        public static GrammarExpr operator +(GrammarExpr lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithAnd(lhs, rhs);
        }
        public static GrammarExpr operator +(GrammarExpr lhs, char rhs)
        {
            return GrammarExpr.AddWithAnd(lhs, GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator +(char lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), rhs);
        }
        public static GrammarExpr operator +(GrammarExpr lhs, string rhs)
        {
            return GrammarExpr.AddWithAnd(lhs, GrammarDef.InPlaceLexRef(rhs));
        }

        public static GrammarExpr operator +(string lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), rhs);
        }
        #endregion

        #region Or
        public static GrammarExpr operator |(GrammarExpr lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithOr(lhs, rhs);
        }
        public static GrammarExpr operator |(GrammarExpr lhs, char rhs)
        {
            return GrammarExpr.AddWithOr(lhs, GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(char lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), rhs);
        }
        public static GrammarExpr operator |(GrammarExpr lhs, string rhs)
        {
            return GrammarExpr.AddWithOr(lhs, GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(string lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), rhs);
        }
        #endregion
    }

    public class GrammarExprFactory : ExprFactory
    {
        public override IAlterationExpr CreateAlteration()
        {
            return new GrammarAlterationExpr();
        }

        public override IProductionExpr CreateProduction()
        {
            return new GrammarProductionExpr();
        }
    }

    public class GrammarAlterationExpr : GrammarExpr, IAlterationExpr
    {
        public GrammarAlterationExpr()
        {
            Expressions = new List<Expr>();
        }

        public List<Expr> Expressions { get; private set; }

        IExpr[] IGroupExpr.Expressions
        {
            get { return Expressions.ToArray(); }
        }

        public virtual void AddExpr(IExpr expr)
        {
            var otherGroup = expr as IGroupExpr;
            if (otherGroup != null)
            {
                //No need to add if there are no expressions in the other group!
                if (otherGroup.Expressions.Length == 0)
                    return;

                //Try getting the unwrapped version if possible!
                var otherGroupUnwrappedExpr = otherGroup.GetUnwrappedSingleExpr();
                if (otherGroupUnwrappedExpr != null)
                {
                    Expressions.Add((Expr)otherGroupUnwrappedExpr);
                    return;
                }

                //Check for alt special case - just append those from that one!
                var otherAltExpr = expr as GrammarAlterationExpr;
                if (otherAltExpr != null && otherAltExpr.CanBeSimplified)
                {
                    //Add all of the child production expressions into this one!
                    foreach (var otherExpr in otherAltExpr.Expressions)
                    {
                        Expressions.Add(otherExpr);
                    }

                    return;
                }
            }

            Expressions.Add((Expr)expr);
        }

        public override string ToStringRef()
        {
            return ToString();
        }

        public override void ToString(StringBuilder sb)
        {
            var expressions = Expressions.ToList();
            var requiresParen = expressions.Count > 1;

            if (requiresParen)
                sb.Append("(");

            var isFirst = true;
            foreach (var expr in expressions)
            {
                if (!isFirst)
                    sb.Append(" | ");

                isFirst = false;

                sb.Append(expr.ToStringRef());
            }

            if (requiresParen)
                sb.Append(")");
        }
    }

    public class GrammarProductionExpr : GrammarExpr, IProductionExpr
    {
        public GrammarProductionExpr()
        {
            Expressions = new List<Expr>();
        }

        public List<Expr> Expressions { get; private set; }

        IExpr[] IGroupExpr.Expressions
        {
            get { return Expressions.ToArray(); }
        }

        public virtual void AddExpr(IExpr expr)
        {
            var otherGroup = expr as IGroupExpr;
            if (otherGroup != null)
            {
                //No need to add if there are no expressions in the other group!
                if (otherGroup.Expressions.Length == 0)
                    return;

                //Try getting the unwrapped version if possible!
                var otherGroupUnwrappedExpr = otherGroup.GetUnwrappedSingleExpr();
                if (otherGroupUnwrappedExpr != null)
                {
                    Expressions.Add((Expr)otherGroupUnwrappedExpr);
                    return;
                }

                //Check for production special case - just append those from that one!
                var otherProdExpr = expr as GrammarProductionExpr;
                if (otherProdExpr != null && otherProdExpr.CanBeSimplified)
                {
                    //Add all of the child production expressions into this one!
                    foreach (var otherExpr in otherProdExpr.Expressions)
                    {
                        Expressions.Add(otherExpr);
                    }

                    return;
                }
            }

            Expressions.Add((Expr)expr);
        }

        public override string ToStringRef()
        {
            return ToString();
        }

        public override void ToString(StringBuilder sb)
        {
            var expressions = Expressions.ToList();

            var requiresParen = expressions.Count > 1;

            if (requiresParen)
                sb.Append("(");

            var isFirst = true;
            foreach (var expr in expressions)
            {
                if (!isFirst)
                    sb.Append(" ");

                isFirst = false;

                sb.Append(expr.ToStringRef());
            }

            if (requiresParen)
                sb.Append(")");
        }
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
