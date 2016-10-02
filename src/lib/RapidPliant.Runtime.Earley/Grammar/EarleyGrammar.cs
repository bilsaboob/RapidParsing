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
    public interface IEarleyGrammar
    {
        IEarleyDfa EarleyDfa { get; }
    }

    public class EarleyGrammar : IEarleyGrammar
    {
        private int LocalLexRuleIndex { get; set; }
        private int LocalParseRuleIndex { get; set; }

        private List<EarleyPatternLexRule> AllLexRules { get; set; }
        private Dictionary<string, EarleyPatternLexRule> LexRulesByPattern { get; set; }

        private List<EarleyParseRule> AllParseRules { get; set; }

        private IEarleyNfa EarleyNfa { get; set; }
        public IEarleyDfa EarleyDfa { get; private set; }

        private EarleyNfaFactory _earleyNfaFactory;
        
        public EarleyGrammar(IGrammarModel grammar)
        {
            Grammar = grammar;

            AllLexRules = new List<EarleyPatternLexRule>();
            LexRulesByPattern = new Dictionary<string, EarleyPatternLexRule>();

            AllParseRules = new List<EarleyParseRule>();

            _earleyNfaFactory = new EarleyNfaFactory();
        }

        public IGrammarModel Grammar { get; private set; }

        public void Compile()
        {
            PreProcessGrammar();

            BuildEarleyAutomata();
        }

        private void BuildEarleyAutomata()
        {
            BuildEarleyNfa();
        }

        private void BuildEarleyNfa()
        {
            var ruleNfas = new List<IEarleyNfa>();

            //Build the rule nfas for each of the expressions!
            var ruleExpressions = Grammar.GetRuleExpressions();

            foreach (var ruleExpr in ruleExpressions)
            {
                var ruleNfa = _earleyNfaFactory.CreateNfaFromExpression(((RuleExpr)ruleExpr).DefinitionExpr);
                ruleNfas.Add(ruleNfa);
            }

            var rulesNfa = _earleyNfaFactory.CreateNfaFromMany(ruleNfas.Cast<EarleyNfa>());

            //Set the complete earley nfa!
            EarleyNfa = rulesNfa;
        }

        private void BuildEarleyDfa()
        {
            EarleyDfa = null;
        }

        private void PreProcessGrammar()
        {
            //Collect the lex rules!
            var lexRules = Grammar.GetLexExpressions();
            foreach (var lexRule in lexRules)
            {
                AddLexRule(lexRule);
            }

            AssignLexRuleIds();

            var parseRules = Grammar.GetRuleExpressions();
            foreach (var parseRule in parseRules)
            {
                AddParseRule(parseRule);
            }

            AssignParseRuleIds();
        }
        
        private void AssignLexRuleIds()
        {
            foreach (var lexRule in AllLexRules)
            {
                lexRule.LocalIndex = LocalLexRuleIndex++;
            }
        }

        private void AssignParseRuleIds()
        {
            foreach (var parseRule in AllParseRules)
            {
                parseRule.LocalIndex = LocalParseRuleIndex++;
            }
        }

        private void AddLexRule(ILexExpr lexExpr)
        {
            var lexPatternExpr = lexExpr as ILexPatternExpr;
            if (lexPatternExpr != null)
            {
                //Create pattern lex rule!
                AddOrMapLexRule(lexPatternExpr.Pattern, lexPatternExpr);
                return;
            }

            var lexSpellingExpr = lexExpr as ILexSpellingExpr;
            if (lexSpellingExpr != null)
            {
                //Create spelling lex rule!
                AddOrMapLexRule(lexSpellingExpr.Spelling, lexSpellingExpr);
                return;
            }

            var lexTerminalExpr = lexExpr as ILexTerminalExpr;
            if (lexTerminalExpr != null)
            {
                //Create terminal lex rule!
                AddOrMapLexRule(lexTerminalExpr.Char.ToString(), lexTerminalExpr);
                return;
            }

            throw new Exception($"Unsupported lex model of type '{lexExpr.GetType().Name}'!");
        }

        private void AddOrMapLexRule(string pattern, ILexExpr lexExpr)
        {
            var lexRule = new EarleyPatternLexRule(pattern, lexExpr);
            AllLexRules.Add(lexRule);

            EarleyPatternLexRule existingLexRule;
            if (!LexRulesByPattern.TryGetValue(pattern, out existingLexRule))
            {
                LexRulesByPattern[pattern] = lexRule;
            }
            else
            {
                existingLexRule.AddMappedLexRule(lexRule);
            }
        }

        private void AddParseRule(IRuleExpr ruleExpr)
        {
            var parseRule = new EarleyParseRule(ruleExpr);
            AllParseRules.Add(parseRule);
        }
    }

    public interface IEarleyLexRule : ILexRule
    {
    }

    public class EarleyPatternLexRule : IEarleyLexRule
    {
        private List<ILexRule> _mappedLexRules;

        public EarleyPatternLexRule(string pattern, ILexExpr lexExpr)
        {
            LexExpr = lexExpr;
            Pattern = pattern;

            _mappedLexRules = new List<ILexRule>();
        }

        public ILexExpr LexExpr { get; private set; }
        public string Pattern { get; private set; }

        public int LocalIndex { get; set; }
        public ILexemeFactory LexemeFactory { get; set; }

        public void AddMappedLexRule(ILexRule otherLexRule)
        {
            if(_mappedLexRules.Contains(otherLexRule))
                _mappedLexRules.Add(otherLexRule);
        }
    }

    public interface IEarleyParseRule : IParseRule
    {
    }

    public class EarleyParseRule : IEarleyParseRule
    {
        public EarleyParseRule(IRuleExpr ruleExpr)
        {
            RuleExpr = ruleExpr;
        }

        public IRuleExpr RuleExpr { get; set; }

        public int LocalIndex { get; set; }
    }
}
