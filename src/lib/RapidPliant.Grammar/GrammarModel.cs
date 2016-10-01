using System.Collections.Generic;
using System.Linq;
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
        protected List<RuleExpr> RuleExpressions { get; set; }
        protected List<LexExpr> LexExpressions { get; set; }

        private ILexModel[] _lexModels;
        private ILexModel[] LexModels
        {
            get
            {
                if (_lexModels == null)
                {
                    _lexModels = LexExpressions.Select(e => e.LexModel).ToArray();
                }
                return _lexModels;
            }
        }

        private IRuleModel[] _ruleModels;
        private IRuleModel[] RuleModels
        {
            get
            {
                if (_ruleModels == null)
                {
                    _ruleModels = RuleExpressions.Select(e => e.RuleModel).ToArray();
                }
                return _ruleModels;
            }
        }

        private GrammarRuleModelBuilder<TGrammarModel> _ruleModelBuilder;

        public GrammarModel()
        {
            AllExpressions = new List<Expr>();
            RuleExpressions = new List<RuleExpr>();
            LexExpressions = new List<LexExpr>();

            StartRuleExpressions = new List<RuleExpr>();

            _ruleModelBuilder = new GrammarRuleModelBuilder<TGrammarModel>(this);
        }

        protected abstract void Define();

        public void Build()
        {
            //Define the grammar!
            Define();

            BuildModels();
        }

        public IEnumerable<ILexModel> GetLexModels()
        {
            return LexModels;
        }

        public IEnumerable<IRuleModel> GetRuleModels()
        {
            return RuleModels;
        }

        #region Build

        private void BuildModels()
        {
            CollectExpressions();

            BuildSymbolModels();
            
            //By now we have all the rule models
        }

        private void BuildSymbolModels()
        {
            foreach (var lexExpr in LexExpressions)
            {
                var lexModel = lexExpr.LexModel;
            }

            foreach (var ruleExpr in RuleExpressions)
            {
                //Set the rule model that was built!
                ruleExpr.RuleModel = BuildRuleModel(ruleExpr);
            }
        }
        
        private IRuleModel BuildRuleModel(RuleExpr ruleExpr)
        {
            return _ruleModelBuilder.BuildRuleModel(ruleExpr);
        }

        private void CollectExpressions()
        {
            foreach (var startRule in StartRuleExpressions)
            {
                CollectExpressions(startRule);
            }
        }

        private void CollectExpressions(Expr expr)
        {
            if (!expr.IsBuilder)
            {
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
                    return;
                }

                return;
            }

            foreach (var alt in expr.Alterations)
            {
                foreach (var altExpr in alt.Expressions)
                {
                    CollectExpressions(altExpr);
                }
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

        internal virtual LexExpr CreateLexPatternExpr(string pattern)
        {
            return new LexExpr(CreateLexPatternModel(pattern));
        }

        internal static ILexModel CreateLexPatternModel(string pattern)
        {
            return new LexPatternModel(pattern);
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

    public abstract class SimpleGrammarModel<TGrammarModel> : GrammarModel<TGrammarModel>
        where TGrammarModel : SimpleGrammarModel<TGrammarModel>
    {
        private NullExpr _nullExpr = new NullExpr();
        protected virtual NullExpr NullExpr { get { return _nullExpr; } }

        #region Customizable helpers

        internal virtual LexExpr CreateNumberExpr(string name = null)
        {
            return new LexExpr(name, CreateLexNumberModel());
        }

        internal virtual ILexModel CreateLexNumberModel()
        {
            return CreateLexPatternModel("[0-9]+");
        }

        internal virtual LexExpr CreateStringQuotedExpr(string name = null)
        {
            return new LexExpr(name, CreateLexStringQuotedModel());
        }

        internal virtual ILexModel CreateLexStringQuotedModel()
        {
            return CreateLexPatternModel("[\"][^\"]+[\"]");
        }
        #endregion

        #region Helpers

        protected NullExpr Null
        {
            get { return NullExpr; }
        }

        protected LexExpr Number(string name = null)
        {
            return CreateNumberExpr(name);
        }

        protected LexExpr StringQuoted(string name = null)
        {
            return CreateStringQuotedExpr(name);
        }

        #endregion
    }
}
