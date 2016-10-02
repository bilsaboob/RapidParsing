using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public partial class Expr : IExpr
    {
        public Expr()
        {
        }

        public string Name { get; set; }

        public ExprOptions Options { get; set; }

        public bool CanBeSimplified
        {
            get
            {
                var opt = Options;
                if (opt == null)
                    return true;

                if (opt.IsOptional)
                    return false;

                if (opt.IsMany)
                    return false;

                return true;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public virtual string ToStringRef()
        {
            return Name;
        }

        public virtual void ToString(StringBuilder sb)
        {
        }
    }

    public class ExprOptions
    {
        public bool IsOptional { get; set; }
        public bool IsMany { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
    }

    public partial class Expr : IExpr
    {
        #region And
        public static Expr operator +(Expr lhs, Expr rhs)
        {
            return AddWithAnd(lhs, rhs);
        }
        public static Expr operator +(Expr lhs, char rhs)
        {
            return AddWithAnd(lhs, new LexTerminalExpr(rhs));
        }
        public static Expr operator +(char lhs, Expr rhs)
        {
            return AddWithAnd(new LexTerminalExpr(lhs), rhs);
        }
        public static Expr operator +(Expr lhs, string rhs)
        {
            return AddWithAnd(lhs, CreateLexExpr(rhs));
        }
        public static Expr operator +(string lhs, Expr rhs)
        {
            return AddWithAnd(CreateLexExpr(lhs), rhs);
        }
        #endregion

        #region Or
        public static Expr operator |(Expr lhs, Expr rhs)
        {
            return AddWithOr(lhs, rhs);
        }
        public static Expr operator |(Expr lhs, char rhs)
        {
            return AddWithOr(lhs, new LexTerminalExpr(rhs));
        }
        public static Expr operator |(char lhs, Expr rhs)
        {
            return AddWithOr(new LexTerminalExpr(lhs), rhs);
        }
        public static Expr operator |(Expr lhs, string rhs)
        {
            return AddWithOr(lhs, CreateLexExpr(rhs));
        }
        public static Expr operator |(string lhs, Expr rhs)
        {
            return AddWithOr(CreateLexExpr(lhs), rhs);
        }

        #endregion

        #region AddWithOr
        protected static Expr AddWithOr(Expr lhsExpr, Expr rhsExpr)
        {
            var altExpr = GetAlteration(lhsExpr);
            altExpr.AddExpr(rhsExpr);
            return altExpr;
        }
        
        #endregion

        #region AddWithAnd
        protected static Expr AddWithAnd(Expr lhsExpr, Expr rhsExpr)
        {
            var prodExpr = GetProduction(lhsExpr);
            prodExpr.AddExpr(rhsExpr);
            return prodExpr;
        }

        #endregion

        #region helpers

        private static ProductionExpr GetProduction(Expr expr)
        {
            var prodExpr = expr as ProductionExpr;
            if (prodExpr == null)
            {
                prodExpr = new ProductionExpr();
                prodExpr.AddExpr(expr);
            }
            return prodExpr; 
        }

        private static AlterationExpr GetAlteration(Expr expr)
        {
            var altExpr = expr as AlterationExpr;
            if (altExpr == null)
            {
                altExpr = new AlterationExpr();
                altExpr.AddExpr(expr);
            }
            return altExpr;
        }

        private static Expr CreateLexExpr(string str)
        {
            LexExpr lexExpr;
            if (str.Length == 1)
            {
                lexExpr = new LexTerminalExpr(str[0]);
            }
            else
            {
                lexExpr = new LexSpellingExpr(str);
            }
            return lexExpr;
        }
        #endregion
    }

    public class GroupExpr : Expr, IGroupExpr
    {
        public GroupExpr()
        {
            Expressions = new List<Expr>();
        }

        public List<Expr> Expressions { get; private set; }

        public virtual void AddExpr(Expr expr)
        {
            Expressions.Add(expr);
        }

        public Expr GetUnwrappedSingleExpr()
        {
            if (!CanBeSimplified)
                return this;

            if (Expressions.Count == 0)
                return null;

            if (Expressions.Count > 1)
                return null;

            var expr = Expressions[0];
            var groupExpr = expr as GroupExpr;
            if (groupExpr != null)
            {
                return groupExpr.GetUnwrappedSingleExpr();
            }
            else
            {
                return expr;
            }
        }

        public override string ToStringRef()
        {
            return ToString();
        }

        IExpr[] IGroupExpr.Expressions
        {
            get { return Expressions.ToArray(); }
        }
    }

    public class AlterationExpr : GroupExpr, IAlterationExpr
    {
        public AlterationExpr()
        {
        }

        public override void AddExpr(Expr expr)
        {
            var otherGroup = expr as GroupExpr;
            if (otherGroup != null)
            {
                //No need to add if there are no expressions in the other group!
                if (otherGroup.Expressions.Count == 0)
                    return;

                //Try getting the unwrapped version if possible!
                var otherGroupUnwrappedExpr = otherGroup.GetUnwrappedSingleExpr();
                if (otherGroupUnwrappedExpr != null)
                {
                    base.AddExpr(otherGroupUnwrappedExpr);
                    return;
                }
                
                //Check for alt special case - just append those from that one!
                var otherAltExpr = expr as AlterationExpr;
                if (otherAltExpr != null && otherAltExpr.CanBeSimplified)
                {
                    //Add all of the child production expressions into this one!
                    foreach (var otherExpr in otherAltExpr.Expressions)
                    {
                        base.AddExpr(otherExpr);
                    }

                    return;
                }
            }

            base.AddExpr(expr);
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

    public class ProductionExpr : GroupExpr, IProductionExpr
    {
        public ProductionExpr()
        {
        }

        public override void AddExpr(Expr expr)
        {
            var otherGroup = expr as GroupExpr;
            if (otherGroup != null)
            {
                //No need to add if there are no expressions in the other group!
                if(otherGroup.Expressions.Count == 0)
                    return;

                //Try getting the unwrapped version if possible!
                var otherGroupUnwrappedExpr = otherGroup.GetUnwrappedSingleExpr();
                if (otherGroupUnwrappedExpr != null)
                {
                    base.AddExpr(otherGroupUnwrappedExpr);
                    return;
                }

                //Check for production special case - just append those from that one!
                var otherProdExpr = expr as ProductionExpr;
                if (otherProdExpr != null && otherProdExpr.CanBeSimplified)
                {
                    //Add all of the child production expressions into this one!
                    foreach (var otherExpr in otherProdExpr.Expressions)
                    {
                        base.AddExpr(otherExpr);
                    }

                    return;
                }
            }

            base.AddExpr(expr);
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
}
