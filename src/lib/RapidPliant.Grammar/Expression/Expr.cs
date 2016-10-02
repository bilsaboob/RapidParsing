using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public interface IExpr
    {
        string Name { get; }
    }

    public interface INullExpr : IExpr
    {
    }

    public interface IGroupExpr : IExpr
    {
        IExpr[] Expressions { get; }
    }

    public interface IAlterationExpr : IGroupExpr
    {
        void AddExpr(IExpr expr);
    }

    public interface IProductionExpr : IGroupExpr
    {
        void AddExpr(IExpr expr);
    }

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
        private static ExprFactory ExprFactory = new ExprFactory();

        #region And
        /*public static Expr operator +(Expr lhs, Expr rhs)
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
        }*/
        #endregion

        #region Or
        /*public static Expr operator |(Expr lhs, Expr rhs)
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
        }*/
        #endregion

        #region AddWithOr
        public static Expr AddWithOr(Expr lhsExpr, Expr rhsExpr, ExprFactory exprFactory = null)
        {
            var altExpr = GetAlteration(lhsExpr, exprFactory);
            altExpr.AddExpr(rhsExpr);
            return (Expr)altExpr;
        }
        
        #endregion

        #region AddWithAnd
        public static Expr AddWithAnd(Expr lhsExpr, Expr rhsExpr, ExprFactory exprFactory = null)
        {
            var prodExpr = GetProduction(lhsExpr, exprFactory);
            prodExpr.AddExpr(rhsExpr);
            return (Expr)prodExpr;
        }

        #endregion

        #region helpers

        private static IProductionExpr GetProduction(Expr expr, ExprFactory exprFactory)
        {
            var prodExpr = expr as IProductionExpr;
            if (prodExpr == null)
            {
                prodExpr = exprFactory.CreateProduction();
                prodExpr.AddExpr(expr);
            }
            return prodExpr; 
        }

        private static IAlterationExpr GetAlteration(Expr expr, ExprFactory exprFactory)
        {
            var altExpr = expr as IAlterationExpr;
            if (altExpr == null)
            {
                altExpr = exprFactory.CreateAlteration();
                altExpr.AddExpr(expr);
            }
            return altExpr;
        }

        /*
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
        */
        #endregion
    }

    public class ExprFactory
    {
        public virtual IProductionExpr CreateProduction()
        {
            return new ProductionExpr();
        }

        public virtual IAlterationExpr CreateAlteration()
        {
            return new AlterationExpr();
        }
    }

    public static class GroupExprExtensions
    {
        public static IExpr GetUnwrappedSingleExpr(this IGroupExpr groupExpr)
        {
            if (!((Expr)groupExpr).CanBeSimplified)
                return groupExpr;

            if (groupExpr.Expressions.Length == 0)
                return null;

            if (groupExpr.Expressions.Length > 1)
                return null;

            var innerExpr = groupExpr.Expressions[0];
            var innerGroupExpr = innerExpr as IGroupExpr;
            if (innerGroupExpr != null)
            {
                return innerGroupExpr.GetUnwrappedSingleExpr();
            }
            else
            {
                return innerExpr;
            }
        }
    }

    public class AlterationExpr : Expr, IAlterationExpr
    {
        public AlterationExpr()
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
                var otherAltExpr = expr as AlterationExpr;
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

    public class ProductionExpr : Expr, IProductionExpr
    {
        public ProductionExpr()
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
                if(otherGroup.Expressions.Length == 0)
                    return;

                //Try getting the unwrapped version if possible!
                var otherGroupUnwrappedExpr = otherGroup.GetUnwrappedSingleExpr();
                if (otherGroupUnwrappedExpr != null)
                {
                    Expressions.Add((Expr)otherGroupUnwrappedExpr);
                    return;
                }

                //Check for production special case - just append those from that one!
                var otherProdExpr = expr as ProductionExpr;
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
}
