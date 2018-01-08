using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Collections;
using RapidPliant.Common.Expression;
using RapidPliant.Grammar.Definitions;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public abstract class GrammarModel<TGrammarModel> : IGrammarModel
        where TGrammarModel : GrammarModel<TGrammarModel>
    {
        protected class GrammarDefEntry
        {
            public GrammarDefEntry(GrammarDef grammarDef)
                : this(null, grammarDef)
            {
            }

            public GrammarDefEntry(GrammarDefMember member, GrammarDef grammarDef)
            {
                Member = member;
                IsDefinedByMember = Member != null;

                if (IsDefinedByMember)
                {
                    MemberGrammarDefName = grammarDef.Name;
                }

                GrammarDef = grammarDef;
            }

            public GrammarDef GrammarDef { get; set; }
            
            public bool IsDefinedByMember { get; private set; }
            public GrammarDefMember Member { get; private set; }

            public string MemberGrammarDefName { get; private set; }
        }

        private List<GrammarDefEntry> _allDefs;
        private List<GrammarDefEntry> _memberDefs;

        private List<GrammarDefEntry> _lexDefs;
        private List<GrammarDefEntry> _ruleDefs;

        private List<GrammarDefEntry> _startRuleDefs;
        private List<GrammarDefMember> _grammarDefMembers;

        private bool _hasBuilt;

        public GrammarModel()
        {
            _allDefs = new List<GrammarDefEntry>();
            _memberDefs = new List<GrammarDefEntry>();
            _ruleDefs = new List<GrammarDefEntry>();
            _lexDefs = new List<GrammarDefEntry>();
            _startRuleDefs = new List<GrammarDefEntry>();

            NextDefinitionId = 1;
        }
        
        protected int NextDefinitionId { get; set; }

        protected List<GrammarDefMember> GrammarDefMembers
        {
            get
            {
                if (_grammarDefMembers == null)
                {
                    _grammarDefMembers = FindGrammarDefMembers();
                }
                return _grammarDefMembers;
            }
        }
        
        public void Build()
        {
            //Start by initializing the definition members
            InitializeGrammarDefMembers();

            //Define the grammar
            Define();

            //Initialize the defined grammar defs - any defs that weren't defined as members... "in place definitions" such as "custom patterns" etc...
            CollectGrammarDefs();

            //Now ensure every definition has a name
            EnsureDefNames();

            //Ensure each expression has an associated "ruledef" owner
            EnsureExpressionOwners();

            AssignIds();

            OnBuilt();

            _hasBuilt = true;
        }

        protected abstract void Define();

        protected virtual void OnBuilt()
        {
        }

        #region IGrammarModel
        public void EnsureBuild()
        {
            if(!_hasBuilt)
                Build();
        }

        public IEnumerable<IRuleDef> GetStartRules()
        {
            return _startRuleDefs.Select(d => d.GrammarDef as IRuleDef).Where(d => d != null).ToList();
        }

        public IGrammarDef GetDefById(int id)
        {
            var defEntry = FindDefEntryById(id);
            if (defEntry == null)
                return null;

            return defEntry.GrammarDef;
        }
        #endregion

        #region AssignIds
        private void AssignIds()
        {
            foreach (var defEntry in _allDefs)
            {
                var def = defEntry.GrammarDef;
                if (def.Id != 0)
                    continue;

                def.Id = GenerateNextDefinitionId();
            }
        }

        protected virtual int GenerateNextDefinitionId()
        {
            return NextDefinitionId++;
        }
        #endregion

        #region InitializeGrammarDefMembers
        private void InitializeGrammarDefMembers()
        {
            var grammarDefMembers = GrammarDefMembers;
            
            foreach (var lexDefMember in grammarDefMembers.Where(m=>m.IsLexDef))
            {
                var grammarDef = InitializeGrammarDef(lexDefMember);
                AddMemberGrammarDef(lexDefMember, grammarDef);
            }
            
            foreach (var ruleDefMember in grammarDefMembers.Where(m => m.IsRuleDef))
            {
                var grammarDef = InitializeGrammarDef(ruleDefMember);
                AddMemberGrammarDef(ruleDefMember, grammarDef);
            }

            foreach (var memberDef in _memberDefs)
            {
                AddDefinition(memberDef);
            }
        }

        private void AddMemberGrammarDef(GrammarDefMember defMember, GrammarDef grammarDef)
        {
            if (_memberDefs.Exists(e => e.Member == defMember))
                return;

            _memberDefs.Add(new GrammarDefEntry(defMember, grammarDef));
        }

        private GrammarDef InitializeGrammarDef(GrammarDefMember defMember)
        {
            var grammarDef = EnsureDefinition(defMember);
            EnsureDefinitionName(grammarDef, defMember.MemberName);
            return grammarDef;
        }
        
        private GrammarDef EnsureDefinition(GrammarDefMember defMember)
        {
            var memberGrammarDef = defMember.GetDef();
            var grammarDef = GetOrCreateGrammarDef(defMember);
            if (grammarDef != memberGrammarDef)
            {
                defMember.SetDef(grammarDef);
            }
            return grammarDef;
        }

        private GrammarDef GetOrCreateGrammarDef(GrammarDefMember defMember)
        {
            var memberGrammarDef = defMember.GetDef();
            
            if (memberGrammarDef == null)
            {
                var defType = defMember.MemberType;

                if (typeof(LexDef).IsAssignableFrom(defType))
                {
                    memberGrammarDef = new LexDef();
                }
                else if (typeof(RuleDef).IsAssignableFrom(defType))
                {
                    memberGrammarDef = new RuleDef();
                }
            }

            if (memberGrammarDef == null)
            {
                throw new Exception($"Unhandled grammar def member '{defMember.MemberName}' of type '{defMember.MemberType.Name}'!");
            }
            
            return (GrammarDef)memberGrammarDef;
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
        #endregion

        #region EnsureDefNames
        private void EnsureDefNames()
        {
            foreach (var defEntry in _allDefs)
            {
                var grammarDef = defEntry.GrammarDef;
                if(!string.IsNullOrEmpty(grammarDef.Name))
                    continue;

                if(!defEntry.IsDefinedByMember)
                    throw new Exception($"Definition must have explicit name if no backing member is defined!");
                
                var initiliaDefinedName = defEntry.MemberGrammarDefName;
                EnsureDefinitionName(grammarDef, initiliaDefinedName);
            }
        }

        #endregion
        
        #region CollectGrammarDefs
        
        private void CollectGrammarDefs()
        {
            //Collect all grammar defs, member definitions as well as "in place" definitions
            var memberRuleDefEntries = _memberDefs.Where(d => d.Member.IsRuleDef).ToList();
            foreach (var memberRuleDefEntry in memberRuleDefEntries)
            {
                //Add the definition to the "all defs"
                if (!AddDefinition(memberRuleDefEntry))
                    continue;

                //Collect grammar defs for the referenced defs in the expression
                var ruleDef = (IRuleDef) memberRuleDefEntry.GrammarDef;
                CollectGrammarDefsForExpression(ruleDef.Expression);
            }
        }

        private bool AddDefinition(GrammarDefEntry defEntry)
        {
            if (_allDefs.Contains(defEntry))
                return false;

            _allDefs.Add(defEntry);

            var grammarDef = defEntry.GrammarDef;

            var ruleDef = grammarDef as IRuleDef;
            if (ruleDef != null)
            {
                _ruleDefs.Add(defEntry);
            }

            var lexDef = grammarDef as ILexDef;
            if (lexDef != null)
            {
                _lexDefs.Add(defEntry);
            }

            return true;
        }

        private GrammarDefEntry FindDefEntryByDef(GrammarDef grammarDef)
        {
            return _allDefs.Find(e => e.GrammarDef == grammarDef);
        }

        private GrammarDefEntry FindDefEntryById(int id)
        {
            return _allDefs.Find(e => e.GrammarDef.Id == id);
        }

        private GrammarDefEntry FindDefEntryByName(string name)
        {
            return _allDefs.Find(e => e.GrammarDef.Name == name);
        }

        protected bool AddDefinition(GrammarDef grammarDef)
        {
            var defEntry = FindDefEntryByDef(grammarDef);
            if (defEntry == null)
            {
                defEntry = new GrammarDefEntry(grammarDef);
                return AddDefinition(defEntry);
            }
            return false;
        }

        private void CollectGrammarDefsForExpression(IExpr expr)
        {
            if (expr.IsGroup)
            {
                if (expr.Expressions == null)
                    return;
                
                //Collect for the children
                foreach (var childExpr in expr.Expressions)
                {
                    CollectGrammarDefsForExpression(childExpr);
                }
            }
            
            var lexRef = expr as ILexRefExpr;
            if (lexRef != null)
            {
                //Just add the definition
                AddDefinition((GrammarDef)lexRef.LexDef);
            }

            var ruleRef = expr as IRuleRefExpr;
            if (ruleRef != null)
            {
                //Add the definition and collect for the rule itself
                var added = AddDefinition((GrammarDef) ruleRef.RuleDef);

                if (added)
                {
                    CollectGrammarDefsForExpression(ruleRef.RuleDef.Expression);
                }
            }
        }

        #endregion

        #region EnsureOwners
        private void EnsureExpressionOwners()
        {
            foreach (var defEntry in _allDefs)
            {
                var ruleDef = defEntry.GrammarDef as IRuleDef;
                if(ruleDef == null)
                    continue;
                
                EnsureExpressionOwner(ruleDef, ruleDef.Expression);
            }
        }

        private void EnsureExpressionOwner(IRuleDef ruleDef, IExpr expr)
        {
            if(expr == null)
                return;

            if (expr.Owner != null)
                return;

            expr.Owner = ruleDef;

            if (expr.IsGroup)
            {
                if (expr.Expressions == null)
                    return;

                //Collect for the children
                foreach (var childExpr in expr.Expressions)
                {
                    EnsureExpressionOwner(ruleDef, childExpr);
                }
            }
            
            var ruleRef = expr as IRuleRefExpr;
            if (ruleRef != null)
            {
                EnsureExpressionOwner(ruleRef.RuleDef, ruleRef.RuleDef.Expression);
            }
        }
        #endregion

        #region Customizable helpers

        protected virtual LexPatternModel CreateLexPatternExpr(string pattern)
        {
            return new LexPatternModel(pattern);
        }

        #endregion

        #region Convenience helpers

        protected void Start(RuleDef startRuleDef)
        {
            var defEntry = FindDefEntryByDef(startRuleDef);
            if(defEntry == null) 
                throw new Exception("No GrammarDef entry found for the specified start rule!");

            _startRuleDefs.Add(defEntry);
        }
        
        protected LexPatternModel LexPattern(string pattern)
        {
            return CreateLexPatternExpr(pattern);
        }

        #endregion

        #region Internal Helpers
        private List<GrammarDefMember> FindGrammarDefMembers()
        {
            var grammarDefMembers = new List<GrammarDefMember>();

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var grammarDefFields = fields.Where(f => typeof(IGrammarDef).IsAssignableFrom(f.FieldType)).ToList();
            grammarDefMembers.AddRange(grammarDefFields.Select(f => new GrammarDefFieldMember(this, f)));

            var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
            var grammarDefProperties = props.Where(f => typeof(IGrammarDef).IsAssignableFrom(f.PropertyType)).ToList();
            grammarDefMembers.AddRange(grammarDefProperties.Select(p => new GrammarDefPropertyMember(this, p)));

            return grammarDefMembers;
        }

        protected abstract class GrammarDefMember
        {
            public GrammarDefMember(IGrammarModel grammarModel, Type memberType)
            {
                GrammarModel = grammarModel;
                MemberType = memberType;

                if (typeof(ILexDef).IsAssignableFrom(MemberType))
                {
                    IsLexDef = true;
                }
                else if (typeof(IRuleDef).IsAssignableFrom(MemberType))
                {
                    IsRuleDef = true;
                }
            }

            public IGrammarModel GrammarModel { get; set; }

            public string MemberName { get; protected set; }

            public Type MemberType { get; protected set; }

            public bool IsLexDef { get; protected set; }

            public bool IsRuleDef { get; protected set; }

            public IGrammarDef GetDef(IGrammarModel grammarModel = null)
            {
                if (grammarModel == null)
                    grammarModel = GrammarModel;

                return GetGrammarDef(grammarModel);
            }

            public void SetDef(IGrammarDef def, IGrammarModel grammarModel = null)
            {
                if (grammarModel == null)
                    grammarModel = GrammarModel;

                SetGrammarDef(grammarModel, def);
            }

            protected abstract IGrammarDef GetGrammarDef(IGrammarModel grammarModel);
            protected abstract void SetGrammarDef(IGrammarModel grammarModel, IGrammarDef def);
        }

        protected class GrammarDefFieldMember : GrammarDefMember
        {
            public GrammarDefFieldMember(IGrammarModel grammarModel, FieldInfo fieldInfo)
                : base(grammarModel, fieldInfo.FieldType)
            {
                FieldInfo = fieldInfo;
                MemberName = fieldInfo.Name;
            }

            public FieldInfo FieldInfo { get; private set; }

            protected override IGrammarDef GetGrammarDef(IGrammarModel grammarModel)
            {
                return (IGrammarDef)FieldInfo.GetValue(grammarModel);
            }

            protected override void SetGrammarDef(IGrammarModel grammarModel, IGrammarDef def)
            {
                FieldInfo.SetValue(grammarModel, def);
            }
        }

        protected class GrammarDefPropertyMember : GrammarDefMember
        {
            public GrammarDefPropertyMember(IGrammarModel grammarModel, PropertyInfo propInfo)
                : base(grammarModel, propInfo.PropertyType)
            {
                PropertyInfo = propInfo;
                MemberName = propInfo.Name;
            }

            public PropertyInfo PropertyInfo { get; private set; }

            protected override IGrammarDef GetGrammarDef(IGrammarModel grammarModel)
            {
                return (IGrammarDef)PropertyInfo.GetValue(grammarModel);
            }

            protected override void SetGrammarDef(IGrammarModel grammarModel, IGrammarDef def)
            {
                PropertyInfo.SetValue(grammarModel, def);
            }
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
