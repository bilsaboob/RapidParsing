using System.Collections.Generic;
using RapidPliant.Util;

namespace RapidPliant.Common.Expression
{
    public interface IExpr
    {
        string Name { get; }

        bool IsGroup { get; }
        bool IsProduction { get; }
        bool IsAlteration { get; }

        bool HasOptions { get; }
        ExprOptions Options { get; }
        
        IReadOnlyList<IExpr> Expressions { get; }

        void AddExpr(IExpr expr);
    }
    
    public abstract partial class Expr<TExpr> : IExpr
        where TExpr : Expr<TExpr>
    {
        protected readonly TExpr _this;
        private bool? _canBeSimplified;

        protected Expr()
            : this(false, false, false)
        {
        }

        protected Expr(bool isGroup, bool isAlteration, bool isProduction)
        {
            _this = (TExpr)this;
            _Expressions = new List<TExpr>();

            IsGroup = isGroup;
            IsAlteration = isAlteration;
            IsProduction = isProduction;
        }

        public string Name { get; set; }
        public ExprOptions Options { get; set; }

        public bool IsGroup { get; private set; }
        public bool IsAlteration { get; private set; }
        public bool IsProduction { get; private set; }

        protected abstract TExpr CreateAlterationExpr();
        protected abstract TExpr CreateProductionExpr();

        public bool HasOptions
        {
            get
            {
                var opt = Options;
                if (opt == null)
                    return false;

                if (opt.IsOptional)
                    return true;

                if (opt.IsMany)
                    return true;

                if (opt.MinCount > 0)
                    return true;

                return false;
            }
        }

        public bool CanBeSimplified
        {
            get
            {
                if (_canBeSimplified.HasValue)
                {
                    return _canBeSimplified.Value;
                }

                var opt = Options;
                if (opt == null)
                    return true;

                if (opt.IsOptional)
                    return false;

                if (opt.IsMany)
                    return false;

                return true;
            }
            set { _canBeSimplified = value; }
        }

        public TExpr GetSimplified()
        {
            return _GetSingleExprUnwrappedFromGroup();
        }

        #region Expr

        protected virtual string _ToStringRef()
        {
            if (!string.IsNullOrEmpty(Name))
                return Name;

            return "<???>";
        }

        protected virtual string _ToStringExpr()
        {
            var text = new StringBuilderText();
            _ToStringExpr(text);
            return text.ToString();
        }

        protected virtual void _ToStringExpr(IText text)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                text.Append(Name);
            }
            else
            {
                text.Append("<???>");
            }
        }

        public override string ToString()
        {
            var text = new StringBuilderText();
            _ToString(text);
            return text.ToString();
        }

        protected virtual void _ToString(IText text)
        {
            if (IsGroup)
            {
                _ToStringGroupExpr(text);
            }
            else
            {
                _ToStringExpr(text);
            }

            if (Options != null)
            {
                Options._ToString(text);
            }
        }

        #endregion

        #region Group expr
        protected List<TExpr> _Expressions { get; private set; }

        public IReadOnlyList<IExpr> Expressions { get { return _Expressions; } }

        public void AddExpr(IExpr expr)
        {
            _AddExpr((TExpr)expr);
        }

        protected void _AddExpr(TExpr expr)
        {
            if (IsAlteration)
            {
                _AddExprAsAlteration(expr);
            }
            else if (IsProduction)
            {
                _AddExprAsProduction(expr);
            }
            else
            {
                _AddExprToList(expr);
            }
        }

        protected void _AddExprAsProduction(TExpr expr)
        {
            if(expr == null)
                return;

            if (expr.IsGroup)
            {
                //No need to add if it's an empty group expression!
                if(expr._Expressions.Count == 0)
                    return;

                //Try getting the unwrapped version if possible!
                var singleExprUnwrapped = expr._GetSingleExprUnwrappedFromGroup();
                if (singleExprUnwrapped != null)
                {
                    _AddExprToListProduction(singleExprUnwrapped);
                    return;
                }

                //Check for alt special case - just append those from that one!
                if (expr.IsProduction && expr.CanBeSimplified)
                {
                    //Add all of the child production expressions into this one!
                    foreach (var otherExpr in expr._Expressions)
                    {
                        _AddExprToListProduction(otherExpr);
                    }

                    return;
                }
            }

            _AddExprToListProduction(expr);
        }

        protected void _AddExprAsAlteration(TExpr expr)
        {
            if (expr == null)
                return;

            if (expr.IsGroup)
            {
                //No need to add if it's an empty group expression!
                if (expr._Expressions.Count == 0)
                    return;

                //Try getting the unwrapped version if possible!
                var singleExprUnwrapped = expr._GetSingleExprUnwrappedFromGroup();
                if (singleExprUnwrapped != null)
                {
                    _AddExprToListAlteration(singleExprUnwrapped);
                    return;
                }

                //Check for alt special case - just append those from that one!
                if (expr.IsAlteration && expr.CanBeSimplified)
                {
                    //Add all of the child production expressions into this one!
                    foreach (var otherExpr in expr._Expressions)
                    {
                        _AddExprToListAlteration(otherExpr);
                    }

                    return;
                }
            }

            _AddExprToListAlteration(expr);
        }

        protected void _AddExprToListAlteration(TExpr expr)
        {
            if (_Expressions.Count > 0)
            {
                var lastExpr = _Expressions[_Expressions.Count - 1];
                if (lastExpr.IsAlteration && lastExpr.CanBeSimplified)
                {
                    lastExpr.AddExpr(expr);
                    return;
                }
            }

            _AddExprToList(expr);
        }

        protected void _AddExprToListProduction(TExpr expr)
        {
            if (_Expressions.Count > 0)
            {
                var lastExpr = _Expressions[_Expressions.Count - 1];
                if (lastExpr.IsAlteration && lastExpr.CanBeSimplified)
                {
                    lastExpr.AddExpr(expr);
                    return;
                }
            }

            _AddExprToList(expr);
        }

        protected void _AddExprToList(TExpr expr, bool optimize = false)
        {
            _Expressions.Add(expr);
        }

        protected TExpr _GetSingleExprUnwrappedFromGroup()
        {
            if (!CanBeSimplified)
                return null;

            if (_Expressions.Count == 0)
                return null;

            if (_Expressions.Count > 1)
                return null;

            var firstExpr = _Expressions[0];
            if (firstExpr.IsGroup)
            {
                var firstExprSimplified = firstExpr._GetSingleExprUnwrappedFromGroup();
                if (firstExprSimplified != null)
                    return firstExprSimplified;
            }

            return firstExpr;
        }

        protected string _ToStringGroupExpr()
        {
            var text = new StringBuilderText();
            _ToStringGroupExpr(text);
            return text.ToString();
        }

        protected virtual void _ToStringGroupExpr(IText text)
        {
            if (IsAlteration)
            {
                _ToStringGroupExprAlteration(text);
                return;
            }

            if(IsProduction)
            {
                _ToStringGroupExprProduction(text);
                return;
            }
        }

        protected string _ToStringGroupExprAlteration()
        {
            var text = new StringBuilderText();
            _ToStringGroupExprAlteration(text);
            return text.Text;
        }

        protected void _ToStringGroupExprAlteration(IText text)
        {
            var expressions = _Expressions;
            var requiresParen = expressions.Count > 1;

            if (requiresParen)
                text.Append("(");

            var isFirst = true;
            foreach (var expr in expressions)
            {
                if (!isFirst)
                    text.Append(" | ");

                isFirst = false;

                text.Append(expr._ToStringRef());
            }

            if (requiresParen)
                text.Append(")");
        }

        protected string _ToStringGroupExprProduction()
        {
            var text = new StringBuilderText();
            _ToStringGroupExprProduction(text);
            return text.Text;
        }

        protected void _ToStringGroupExprProduction(IText text)
        {
            var expressions = _Expressions;
            var requiresParen = expressions.Count > 1;

            if (requiresParen)
                text.Append("(");

            var isFirst = true;
            foreach (var expr in expressions)
            {
                if (!isFirst)
                    text.Append(" ");

                isFirst = false;

                text.Append(expr._ToStringRef());
            }

            if (requiresParen)
                text.Append(")");
        }

        #endregion
    }

    public class ExprOptions
    {
        public bool IsOptional { get; set; }
        public bool IsMany { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }

        public void _ToString(IText text)
        {
            if (IsMany)
            {
                if (IsOptional)
                {
                    text.Append("*");
                }
                else
                {
                    text.Append("+");
                }
            }
            else if (IsOptional)
            {
                text.Append("?");
            }
        }
    }

    public abstract partial class Expr<TExpr> : IExpr
    {
        #region AddWithOr
        public static TExpr AddWithOr(TExpr lhsExpr, TExpr rhsExpr)
        {
            var altExpr = GetAlteration(lhsExpr);
            altExpr._AddExpr(rhsExpr);
            return altExpr;
        }
        
        #endregion

        #region AddWithAnd
        public static TExpr AddWithAnd(TExpr lhsExpr, TExpr rhsExpr)
        {
            var prodExpr = GetProduction(lhsExpr);
            prodExpr._AddExpr(rhsExpr);
            return prodExpr;
        }

        #endregion

        #region helpers

        private static TExpr GetProduction(TExpr expr)
        {
            if (expr.IsProduction)
                return expr;

            var prodExpr = expr.CreateProductionExpr();
            prodExpr._AddExpr(expr);
            return prodExpr; 
        }

        private static TExpr GetAlteration(TExpr expr)
        {
            if (expr.IsAlteration)
                return expr;

            var altExpr = expr.CreateAlterationExpr();
            altExpr._AddExpr(expr);
            return altExpr;
        }
        #endregion
    }
}
