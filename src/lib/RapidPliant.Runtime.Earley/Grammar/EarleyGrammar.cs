using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar;
using RapidPliant.Grammar.Expression;
using RapidPliant.Runtime.Earley.Automata.Earley;
using RapidPliant.Runtime.Earley.Lexing;

namespace RapidPliant.Runtime.Earley.Grammar
{
    public class EarleyGrammar : IEarleyGrammar
    {
        private int LocalLexDefIndex { get; set; }
        private int LocalRuleDefIndex { get; set; }

        private List<EarleyPatternLexDef> AllLexDefs { get; set; }
        private Dictionary<string, EarleyPatternLexDef> LexDefsByName { get; set; }
        private Dictionary<string, EarleyPatternLexDef> LexDefsByPattern { get; set; }

        private List<EarleyRuleDef> AllRuleDefs { get; set; }
        private Dictionary<string, EarleyRuleDef> RuleDefsByName { get; set; }
        private List<EarleyRuleDef> StartRuleDefs { get; set; }

        public EarleyGrammar(IGrammarModel grammar)
        {
            Grammar = grammar;

            AllLexDefs = new List<EarleyPatternLexDef>();
            LexDefsByName = new Dictionary<string, EarleyPatternLexDef>();
            LexDefsByPattern = new Dictionary<string, EarleyPatternLexDef>();

            AllRuleDefs = new List<EarleyRuleDef>();
            RuleDefsByName = new Dictionary<string, EarleyRuleDef>();

            StartRuleDefs = new List<EarleyRuleDef>();
        }

        public IGrammarModel Grammar { get; private set; }

        public IEarleyDfa EarleyDfa { get; private set; }

        public IEnumerable<IEarleyRuleDef> GetStartRules()
        {
            return StartRuleDefs;
        }

        public void Compile()
        {
            PreProcessGrammar();

            BuildEarleyAutomata();
        }

        private void BuildEarleyAutomata()
        {
            var dfaFactory = new EarleyDfaFactory();
            dfaFactory.Create(this);
        }
        
        private void PreProcessGrammar()
        {
            //Collect the lex rules!
            //PreProcessLexDefinitions();

            PreProcessRuleDefinitions();

            PreProcessStartRules();

            foreach (var earleyRuleDef in AllRuleDefs)
            {
                earleyRuleDef.Build();
            }
        }

        private void PreProcessStartRules()
        {
            var startRuleDefs = Grammar.GetStartRules();
            foreach (var startRuleDef in startRuleDefs)
            {
                var ruleDef = GetRuleDefByName(startRuleDef.Name);
                if (ruleDef == null)
                {
                    throw new Exception($"Could not find start rule def '{startRuleDef.Name}'");
                }

                if(!StartRuleDefs.Contains(ruleDef))
                    StartRuleDefs.Add(ruleDef);
            }
        }

        private void PreProcessLexDefinitions()
        {
            var lexDefs = Grammar.GetLexDefinitions();
            foreach (var lexDef in lexDefs)
            {
                AddLexDef(lexDef);
            }

            foreach (var lexRule in AllLexDefs)
            {
                lexRule.LocalIndex = LocalLexDefIndex++;
            }
        }
        
        private void PreProcessRuleDefinitions()
        {
            var ruleDefs = Grammar.GetRuleDefinitions();
            foreach (var ruleDef in ruleDefs)
            {
                AddRuleDef(ruleDef);
            }

            foreach (var parseRule in AllRuleDefs)
            {
                parseRule.LocalIndex = LocalRuleDefIndex++;
            }
        }

        #region helpers
        private void AddLexDef(ILexDef lexDef)
        {
            var lexModel = lexDef.LexModel;

            var lexPatternExpr = lexModel as ILexPatternModel;
            if (lexPatternExpr != null)
            {
                //Create pattern lex rule!
                AddOrMapLexRule(lexPatternExpr.Pattern, lexDef);
                return;
            }

            var lexSpellingExpr = lexModel as ILexSpellingModel;
            if (lexSpellingExpr != null)
            {
                //Create spelling lex rule!
                AddOrMapLexRule(lexSpellingExpr.Spelling, lexDef);
                return;
            }

            var lexTerminalExpr = lexModel as ILexTerminalModel;
            if (lexTerminalExpr != null)
            {
                //Create terminal lex rule!
                AddOrMapLexRule(lexTerminalExpr.Char.ToString(), lexDef);
                return;
            }

            throw new Exception($"Unsupported lex model of type '{lexModel.GetType().Name}'!");
        }

        private void AddOrMapLexRule(string pattern, ILexDef lexDef)
        {
            var earleyLexDef = new EarleyPatternLexDef(pattern, lexDef);

            LexDefsByName.Add(lexDef.Name, earleyLexDef);
            AllLexDefs.Add(earleyLexDef);

            EarleyPatternLexDef existingEarleyLexDef;
            if (!LexDefsByPattern.TryGetValue(pattern, out existingEarleyLexDef))
            {
                LexDefsByPattern[pattern] = earleyLexDef;
            }
            else
            {
                existingEarleyLexDef.AddMappedLexRule(earleyLexDef);
            }
        }

        private void AddRuleDef(IRuleDef ruleDef)
        {
            var earleyRuleDef = new EarleyRuleDef(ruleDef);

            RuleDefsByName.Add(ruleDef.Name, earleyRuleDef);
            AllRuleDefs.Add(earleyRuleDef);
        }

        private EarleyRuleDef GetRuleDefByName(string ruleDefName)
        {
            EarleyRuleDef earleyRuleDef;
            RuleDefsByName.TryGetValue(ruleDefName, out earleyRuleDef);
            return earleyRuleDef;
        }
        #endregion
    }
    
    public class EarleyPatternLexDef : IEarleyLexDef
    {
        private List<IEarleyLexDef> _mappedLexDefs;

        public EarleyPatternLexDef(string pattern, ILexDef lexDef)
        {
            LexDef = lexDef;
            Pattern = pattern;

            _mappedLexDefs = new List<IEarleyLexDef>();
        }

        public ILexDef LexDef { get; private set; }
        public string Pattern { get; private set; }

        public int LocalIndex { get; set; }
        public ILexemeFactory LexemeFactory { get; set; }

        public void AddMappedLexRule(IEarleyLexDef otherEarleyLexDef)
        {
            if(_mappedLexDefs.Contains(otherEarleyLexDef))
                _mappedLexDefs.Add(otherEarleyLexDef);
        }
    }
}
