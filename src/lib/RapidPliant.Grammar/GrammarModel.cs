using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Grammar.Definitions;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public abstract class GrammarModel<TGrammarModel> : IGrammarModel
        where TGrammarModel : GrammarModel<TGrammarModel>
    {
        protected GrammarDefinitionCollecion<RuleDef, IRuleDef> StartRuleDefinitions { get; set; }
        protected List<GrammarDef> AllDefinitions { get; set; }

        protected GrammarDefinitionCollecion<RuleDef, IRuleDef> RuleDefinitions { get; set; }
        protected GrammarDefinitionCollecion<LexDef, ILexDef> LexDefinitions { get; set; }

        private Dictionary<string, GrammarDef> _preBuildDefinitions;
        
        public GrammarModel()
        {
            AllDefinitions = new List<GrammarDef>();
            RuleDefinitions = new GrammarDefinitionCollecion<RuleDef, IRuleDef>();
            LexDefinitions = new GrammarDefinitionCollecion<LexDef, ILexDef>();

            StartRuleDefinitions = new GrammarDefinitionCollecion<RuleDef, IRuleDef>();

            _preBuildDefinitions = new Dictionary<string, GrammarDef>();
        }

        protected abstract void Define();

        public void Build()
        {
            InitializeExpressions();

            //Define the grammar!
            Define();

            CollectDefinitions();

            EnsureExpressionNames();

            OnBuilt();
        }

        private void InitializeExpressions()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var exprFields = fields.Where(f => typeof(IExpr).IsAssignableFrom(f.FieldType)).ToList();
            foreach (var exprField in exprFields)
            {
                var expr = EnsureDefinition(exprField.GetValue(this), exprField.FieldType);
                exprField.SetValue(this, expr);
                EnsureDefinitionName(expr, exprField.Name);
                _preBuildDefinitions[exprField.Name] = expr;
            }

            var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var exprProps = props.Where(f => typeof(IExpr).IsAssignableFrom(f.PropertyType)).ToList();
            foreach (var exprProp in exprProps)
            {
                try
                {
                    var expr = EnsureDefinition(exprProp.GetValue(this), exprProp.PropertyType);
                    exprProp.SetValue(this, expr);
                    EnsureDefinitionName(expr, exprProp.Name);
                    _preBuildDefinitions[exprProp.Name] = expr;
                }
                catch (Exception)
                {
                    //Doesn't matter
                }
            }
        }

        private GrammarDef EnsureDefinition(object exprObj, Type exprType)
        {
            if (exprObj == null)
            {
                if (typeof(LexDef).IsAssignableFrom(exprType))
                {
                    return new LexDef();
                }

                if (typeof(RuleDef).IsAssignableFrom(exprType))
                {
                    return new RuleDef();
                }
            }

            return (GrammarDef)exprObj;
        }

        private void EnsureDefinitionName(GrammarDef grammarDef, string name)
        {
            if (string.IsNullOrEmpty(grammarDef.Name))
            {
                if (name.Length > 1 && name[0] == 'm' && char.IsUpper(name[1]))
                {
                    name = name.Substring(1);
                }

                //Set the expression name to the name of the field
                grammarDef.Name = name.Trim(' ', '_');
            }
        }

        private void EnsureExpressionNames()
        {
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var defFields = fields.Where(f => typeof(GrammarDef).IsAssignableFrom(f.FieldType)).ToList();
            foreach (var defField in defFields)
            {
                var def = defField.GetValue(this) as GrammarDef;
                if(def == null)
                    continue;

                var name = GetDeclaredDefinitionName(defField.Name);
                EnsureDefinitionName(def, name);
            }

            var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var defProps = props.Where(f => typeof(GrammarDef).IsAssignableFrom(f.PropertyType)).ToList();
            foreach (var defProp in defProps)
            {
                var def = defProp.GetValue(this) as GrammarDef;
                if (def == null)
                    continue;

                var name = GetDeclaredDefinitionName(defProp.Name);
                EnsureDefinitionName(def, name);
            }
        }

        private string GetDeclaredDefinitionName(string memberName)
        {
            GrammarDef declDef;
            if (_preBuildDefinitions.TryGetValue(memberName, out declDef))
            {
                return declDef.Name ?? memberName;
            }
            return memberName;
        }

        public IEnumerable<ILexDef> GetLexDefinitions()
        {
            return LexDefinitions.External;
        }

        public IEnumerable<IRuleDef> GetRuleDefinitions()
        {
            return RuleDefinitions.External;
        }

        public IEnumerable<IRuleDef> GetStartRules()
        {
            return StartRuleDefinitions.External;
        }

        protected virtual void OnBuilt()
        {
        }

        #region Build


        private void CollectDefinitions()
        {
            foreach (var startRule in StartRuleDefinitions)
            {
                if (!AddDefinition(startRule))
                    return;

                CollectDefinitions(startRule.Expression);
            }
        }

        private void CollectDefinitions(IExpr expr)
        {
            if (expr.IsGroup)
            {
                foreach (var childExpr in expr.Expressions)
                {
                    CollectDefinitions(childExpr);
                }

                return;
            }
            
            var lexRef = expr as ILexRefExpr;
            if (lexRef != null)
            {
                AddLexDefinition(lexRef.LexDef);
                return;
            }

            var ruleRef = expr as IRuleRefExpr;
            if (ruleRef != null)
            {
                AddRuleDefinition(ruleRef.RuleDef);
                CollectDefinitions(ruleRef.RuleDef.Expression);
                return;
            }
        }

        private bool AddDefinition(GrammarDef def)
        {
            if (!AllDefinitions.Contains(def))
            {
                AllDefinitions.Add(def);
                return true;
            }

            return false;
        }

        private bool AddRuleDefinition(IRuleDef ruleDef)
        {
            if (!RuleDefinitions.Contains(ruleDef))
            {
                RuleDefinitions.Add(ruleDef);
                return true;
            }

            return false;
        }

        private bool AddLexDefinition(ILexDef lexDef)
        {
            if (!LexDefinitions.Contains(lexDef))
            {
                LexDefinitions.Add(lexDef);
                return true;
            }

            return false;
        }
        #endregion

        #region Customizable helpers

        internal virtual LexPatternModel CreateLexPatternExpr(string pattern)
        {
            return new LexPatternModel(pattern);
        }

        #endregion

        #region Helpers

        protected void Start(RuleDef startRuleExpr)
        {
            StartRuleDefinitions.Add(startRuleExpr);
        }

        protected LexPatternModel LexPattern(string pattern)
        {
            return CreateLexPatternExpr(pattern);
        }

        #endregion
    }

    public class GrammarDefinitionCollecion<TDefInternal, TDefExternal> : IEnumerable<TDefInternal>
        where TDefInternal : TDefExternal 
    {
        private HashSet<TDefInternal> _internal;
        private TDefExternal[] _external;

        public GrammarDefinitionCollecion()
        {
            _internal = new HashSet<TDefInternal>();
        }

        public TDefExternal[] External
        {
            get
            {
                if (_external == null)
                {
                    _external = _internal.Select(e => (TDefExternal)e).ToArray();
                }

                return _external;
            }
        }

        public void Add(TDefInternal def)
        {
            _internal.Add(def);
            _external = null;
        }

        public void Add(TDefExternal def)
        {
            Add((TDefInternal) def);
        }

        public bool Contains(TDefInternal def)
        {
            return _internal.Contains(def);
        }

        public bool Contains(TDefExternal def)
        {
            return Contains((TDefInternal) def);
        }

        public IEnumerator<TDefInternal> GetEnumerator()
        {
            return _internal.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
