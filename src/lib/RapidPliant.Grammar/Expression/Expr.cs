using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public partial class Expr
    {
        protected IGrammarModel Grammar { get; set; }
        
        public Expr()
        {
            Grammar = null;
            IsInitialize = false;
            Alterations = new ExprAlterations();
            IsBuilder = true;
        }

        public bool IsBuilder { get; set; }

        public bool IsInitialize { get; private set; }

        public ExprAlterations Alterations { get; private set; }
        
        public void AddExpr(Expr expr)
        {
            Alterations.AddExpr(expr);
        }

        public void AddAlteration(ExprAlteration alteration)
        {
            Alterations.AddAlteration(alteration);
        }

        public virtual IRuleModel ToRuleModel()
        {
            return null;
        }

        public virtual ILexModel ToLexModel()
        {
            return null;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public virtual void ToString(StringBuilder sb)
        {
            var isFirst = true;
            foreach (var alt in Alterations)
            {
                if (!isFirst)
                    sb.Append(" | ");

                isFirst = false;

                var needParens = alt.Count > 1;
                if (needParens)
                {
                    sb.Append("(");
                }

                alt.ToString(sb);

                if (needParens)
                {
                    sb.Append(")");
                }
            }
        }
    }

    public partial class Expr
    {

        #region And
        public static Expr operator +(Expr lhs, Expr rhs)
        {
            return AddWithAnd(lhs, rhs);
        }
        public static Expr operator +(Expr lhs, char rhs)
        {
            return AddWithAnd(lhs, new LexTerminalModel(rhs));
        }
        public static Expr operator +(char lhs, Expr rhs)
        {
            return AddWithAnd(new LexTerminalModel(lhs), rhs);
        }
        #endregion

        #region Or
        public static Expr operator |(Expr lhs, Expr rhs)
        {
            return AddWithOr(lhs, rhs);
        }
        public static Expr operator |(Expr lhs, char rhs)
        {
            return AddWithOr(lhs, new LexTerminalModel(rhs));
        }
        public static Expr operator |(char lhs, Expr rhs)
        {
            return AddWithOr(new LexTerminalModel(lhs), rhs);
        }
        public static Expr operator |(Expr lhs, string rhs)
        {
            ILexModel rhsLexModel;
            if (rhs.Length == 1)
            {
                rhsLexModel = new LexTerminalModel(rhs[0]);
            }
            else
            {
                rhsLexModel = new LexSpellingModel(rhs);
            }
            return AddWithOr(lhs, rhsLexModel);
        }
        public static Expr operator |(string lhs, Expr rhs)
        {
            ILexModel lhsLexModel;
            if (lhs.Length == 1)
            {
                lhsLexModel = new LexTerminalModel(lhs[0]);
            }
            else
            {
                lhsLexModel = new LexSpellingModel(lhs);
            }
            return AddWithOr(lhsLexModel, rhs);
        }
        #endregion

        #region AddWithOr
        private static Expr AddWithOr(ILexModel lhs, Expr rhsExpr)
        {
            return AddWithOr(new LexExpr(lhs), rhsExpr);
        }
        private static Expr AddWithOr(Expr lhsExpr, ILexModel rhs)
        {
            return AddWithOr(lhsExpr, new LexExpr(rhs));
        }
        private static Expr AddWithOr(Expr lhsExpr, Expr rhsExpr)
        {
            var expr = GetBuilderExpr(lhsExpr);

            if (rhsExpr.IsBuilder)
            {
                //Append the alterations as they are
                foreach (var rhsAlteration in rhsExpr.Alterations)
                {
                    expr.AddAlteration(rhsAlteration);
                }
            }
            else
            {
                expr.Alterations.BeginNewAlteration();
                expr.AddExpr(rhsExpr);
            }
            
            return expr;
        }
        #endregion

        #region AddWithAnd
        private static Expr AddWithAnd(ILexModel lhs, Expr rhsExpr)
        {
            return AddWithAnd(new LexExpr(lhs), rhsExpr);
        }
        private static Expr AddWithAnd(Expr lhsExpr, ILexModel rhs)
        {
            return AddWithAnd(lhsExpr, new LexExpr(rhs));
        }
        private static Expr AddWithAnd(Expr lhsExpr, Expr rhsExpr)
        {
            var expr = GetBuilderExpr(lhsExpr);
            expr.AddExpr(rhsExpr);
            return expr;
        }

        private static Expr GetBuilderExpr(Expr expr)
        {
            if (expr.IsBuilder)
                return expr;

            var builderExpr = new Expr();
            builderExpr.AddExpr(expr);
            return builderExpr;
        }
        #endregion
    }

    public class ExprAlterations : IEnumerable<ExprAlteration>
    {
        private List<ExprAlteration> _alterations;
        private ExprAlteration _alteration;

        public ExprAlterations()
        {
            _alterations = new List<ExprAlteration>();
        }

        public int Count { get { return _alterations.Count; } }

        public ExprAlteration this[int index]
        {
            get { return _alterations[index]; }
        }

        public void BeginNewAlteration()
        {
            if (_alteration == null)
            {
                _alteration = new ExprAlteration();
                _alterations.Add(_alteration);
            }
            else
            {
                if (_alteration.Count == 0)
                {
                    return;
                }
                else
                {
                    _alteration = new ExprAlteration();
                    _alterations.Add(_alteration);
                }
            }
        }
        
        public void AddExpr(Expr expr)
        {
            EnsureAlterationExists();
            _alteration.AddExpr(expr);
        }

        public void AddAlteration(ExprAlteration alteration)
        {
            if (_alteration == null || _alteration.Count == 0)
            {
                _alterations.Remove(_alteration);
                _alteration = alteration;
                _alterations.Add(_alteration);
            }
            else
            {
                _alterations.Add(alteration);
                _alteration = alteration;
            }
        }

        private void EnsureAlterationExists()
        {
            if (_alteration == null)
            {
                _alteration = new ExprAlteration();
                _alterations.Add(_alteration);
            }
        }

        public IEnumerator<ExprAlteration> GetEnumerator()
        {
            return _alterations.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ExprAlteration
    {
        private List<Expr> _expressions;

        public ExprAlteration()
        {
            _expressions = new List<Expr>();
        }

        public List<Expr> Expressions { get { return _expressions.ToList(); } }

        public int Count { get { return _expressions.Count; } }
        
        public void AddExpr(Expr expr)
        {
            _expressions.Add(expr);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public virtual void ToString(StringBuilder sb)
        {
            var isFirst = true;
            foreach (var expr in _expressions)
            {
                if (!isFirst)
                    sb.Append(" ");

                isFirst = false;

                var ruleExpr = expr as RuleExpr;
                if (ruleExpr != null)
                {
                    sb.Append(ruleExpr.Name);
                    continue;
                }

                var lexExpr = expr as LexExpr;
                if (lexExpr != null)
                {
                    lexExpr.ToString(sb);
                    continue;
                }

                var nullExpr = expr as NullExpr;
                if (nullExpr != null)
                {
                    nullExpr.ToString(sb);
                    continue;
                }

                sb.Append("(");
                expr.ToString(sb);
                sb.Append(")");
            }
        }
    }
}
