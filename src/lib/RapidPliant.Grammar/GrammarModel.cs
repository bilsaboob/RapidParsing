using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public abstract class GrammarModel<TGrammarModel> : IGrammarModel
        where TGrammarModel : GrammarModel<TGrammarModel>
    {
        protected List<RuleExpr> StartRuleExpressions { get; set; }
        protected List<Expr> AllExpressions { get; set; }

        protected GrammarExprCollection<RuleExpr, IRuleExpr> RuleExpressions { get; set; }
        protected GrammarExprCollection<LexExpr, ILexExpr> LexExpressions { get; set; }

        private Dictionary<string, Expr> _declarationExpressions;
        
        public GrammarModel()
        {
            AllExpressions = new List<Expr>();
            RuleExpressions = new GrammarExprCollection<RuleExpr, IRuleExpr>();
            LexExpressions = new GrammarExprCollection<LexExpr, ILexExpr>();

            StartRuleExpressions = new List<RuleExpr>();

            _declarationExpressions = new Dictionary<string, Expr>();
        }

        protected abstract void Define();

        public void Build()
        {
            InitializeExpressions();

            //Define the grammar!
            Define();

            CollectExpressions();

            EnsureExpressionNames();

            OnBuilt();
        }

        private void InitializeExpressions()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var exprFields = fields.Where(f => typeof(IExpr).IsAssignableFrom(f.FieldType)).ToList();
            foreach (var exprField in exprFields)
            {
                var expr = EnsureExpr(exprField.GetValue(this), exprField.FieldType);
                exprField.SetValue(this, expr);
                EnsureExprName(expr, exprField.Name);
                _declarationExpressions[exprField.Name] = expr;
            }

            var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var exprProps = props.Where(f => typeof(IExpr).IsAssignableFrom(f.PropertyType)).ToList();
            foreach (var exprProp in exprProps)
            {
                try
                {
                    var expr = EnsureExpr(exprProp.GetValue(this), exprProp.PropertyType);
                    exprProp.SetValue(this, expr);
                    EnsureExprName(expr, exprProp.Name);
                    _declarationExpressions[exprProp.Name] = expr;
                }
                catch (Exception)
                {
                    //Doesn't matter
                }
            }
        }

        private Expr EnsureExpr(object exprObj, Type exprType)
        {
            if (exprObj == null)
            {
                if (typeof(LexExpr).IsAssignableFrom(exprType))
                {
                    return new DeclarationLexExpr(null);
                }

                if (typeof(RuleExpr).IsAssignableFrom(exprType))
                {
                    return new DeclarationRuleExpr(null);
                }
            }

            return (Expr) exprObj;
        }

        private void EnsureExprName(Expr expr, string name)
        {
            if (string.IsNullOrEmpty(expr.Name))
            {
                if (name.Length > 1 && name[0] == 'm' && char.IsUpper(name[1]))
                {
                    name = name.Substring(1);
                }

                //Set the expression name to the name of the field
                expr.Name = name.Trim(' ', '_');
            }
        }

        private void EnsureExpressionNames()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var exprFields = fields.Where(f => typeof(IExpr).IsAssignableFrom(f.FieldType)).ToList();
            foreach (var exprField in exprFields)
            {
                var expr = exprField.GetValue(this) as Expr;
                if(expr == null)
                    continue;

                var name = GetDeclaredExpressionName(exprField.Name);
                EnsureExprName(expr, name);
            }

            var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var exprProps = props.Where(f => typeof(IExpr).IsAssignableFrom(f.PropertyType)).ToList();
            foreach (var exprProp in exprProps)
            {
                var expr = exprProp.GetValue(this) as Expr;
                if (expr == null)
                    continue;

                var name = GetDeclaredExpressionName(exprProp.Name);
                EnsureExprName(expr, name);
            }
        }

        private string GetDeclaredExpressionName(string memberName)
        {
            Expr declExpr;
            if (_declarationExpressions.TryGetValue(memberName, out declExpr))
            {
                return declExpr.Name ?? memberName;
            }
            return memberName;
        }

        public IEnumerable<ILexExpr> GetLexExpressions()
        {
            return LexExpressions.External;
        }

        public IEnumerable<IRuleExpr> GetRuleExpressions()
        {
            return RuleExpressions.External;
        }

        protected virtual void OnBuilt()
        {
        }

        #region Build


        private void CollectExpressions()
        {
            foreach (var startRule in StartRuleExpressions)
            {
                CollectExpressions(startRule);
            }
        }

        private void CollectExpressions(Expr expr)
        {
            var groupExpr = expr as GroupExpr;
            if (groupExpr != null)
            {
                foreach (var childExpr in groupExpr.Expressions)
                {
                    CollectExpressions(childExpr);
                }

                return;
            }

            if (!AddExpr(expr))
                return;

            var lexExpr = expr as LexExpr;
            if (lexExpr != null)
            {
                AddLexExpression(lexExpr);
                return;
            }

            var ruleExpr = expr as RuleExpr;
            if (ruleExpr != null)
            {
                AddRuleExpr(ruleExpr);
                CollectExpressions(ruleExpr.DefinitionExpr);
            }
        }

        private bool AddExpr(Expr expr)
        {
            if (!AllExpressions.Contains(expr))
            {
                AllExpressions.Add(expr);
                return true;
            }

            return false;
        }

        private bool AddRuleExpr(RuleExpr ruleExpr)
        {
            if (!RuleExpressions.Contains(ruleExpr))
            {
                RuleExpressions.Add(ruleExpr);
                return true;
            }

            return false;
        }

        private bool AddLexExpression(LexExpr lexExpr)
        {
            if (!LexExpressions.Contains(lexExpr))
            {
                LexExpressions.Add(lexExpr);
                return true;
            }

            return false;
        }
        #endregion

        #region Customizable helpers

        internal virtual LexPatternExpr CreateLexPatternExpr(string pattern)
        {
            return new LexPatternExpr(pattern);
        }

        #endregion

        #region Helpers

        protected void Start(RuleExpr startRuleExpr)
        {
            StartRuleExpressions.Add(startRuleExpr);
        }

        protected LexExpr LexPattern(string pattern)
        {
            return CreateLexPatternExpr(pattern);
        }

        #endregion
    }

    public class GrammarExprCollection<TExprInternal, TExprExternal> : IEnumerable<TExprInternal>
        where TExprInternal : TExprExternal 
    {
        private List<TExprInternal> _expressionsInternal;
        private TExprExternal[] _expressionsExternal;

        public GrammarExprCollection()
        {
            _expressionsInternal = new List<TExprInternal>();
        }

        public TExprExternal[] External
        {
            get
            {
                if (_expressionsExternal == null)
                {
                    _expressionsExternal = _expressionsInternal.Select(e => (TExprExternal)e).ToArray();
                }

                return _expressionsExternal;
            }
        }

        public void Add(TExprInternal expr)
        {
            _expressionsInternal.Add(expr);
            _expressionsExternal = null;
        }

        public IEnumerator<TExprInternal> GetEnumerator()
        {
            return _expressionsInternal.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
