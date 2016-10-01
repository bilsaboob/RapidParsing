using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar;
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
        private List<EarleyPatternLexRule> AllLexRules { get; set; }
        private Dictionary<string, EarleyPatternLexRule> LexRulesByPattern { get; set; }

        private IEarleyNfa EarleyNfa { get; set; }
        public IEarleyDfa EarleyDfa { get; private set; }
        
        public EarleyGrammar(IGrammarModel grammar)
        {
            Grammar = grammar;

            AllLexRules = new List<EarleyPatternLexRule>();
            LexRulesByPattern = new Dictionary<string, EarleyPatternLexRule>();
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
            EarleyNfa = null;
        }

        private void BuildEarleyDfa()
        {
            EarleyDfa = null;
        }

        private void PreProcessGrammar()
        {
            //Collect the lex rules!
            var lexRules = Grammar.GetLexModels();
            foreach (var lexRule in lexRules)
            {
                AddLexRule(lexRule);
            }

            AssignLexRuleIds();
        }

        private void AssignLexRuleIds()
        {
            foreach (var lexRule in AllLexRules)
            {
                lexRule.LocalIndex = LocalLexRuleIndex++;
            }
        }

        private void AddLexRule(ILexModel lexModel)
        {
            var patternLexModel = lexModel as ILexPatternModel;
            if (patternLexModel != null)
            {
                //Create pattern lex rule!
                AddOrMapLexRule(patternLexModel.Pattern, patternLexModel);
                return;
            }

            var spellingLexModel = lexModel as ILexSpellingModel;
            if (spellingLexModel != null)
            {
                //Create spelling lex rule!
                AddOrMapLexRule(spellingLexModel.Spelling, spellingLexModel);
                return;
            }

            var terminalLexModel = lexModel as ILexTerminalModel;
            if (terminalLexModel != null)
            {
                //Create terminal lex rule!
                AddOrMapLexRule(terminalLexModel.Char.ToString(), terminalLexModel);
                return;
            }

            throw new Exception($"Unsupported lex model of type '{lexModel.GetType().Name}'!");
        }

        private void AddOrMapLexRule(string pattern, ILexModel lexModel)
        {
            var lexRule = new EarleyPatternLexRule(pattern, lexModel);
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
    }

    public class EarleyPatternLexRule : ILexRule
    {
        private List<ILexRule> _mappedLexRules;

        public EarleyPatternLexRule(string pattern, ILexModel lexModel)
        {
            LexModel = lexModel;
            Pattern = pattern;

            _mappedLexRules = new List<ILexRule>();
        }

        public string Pattern { get; private set; }
        public ILexModel LexModel { get; private set; }

        public int LocalIndex { get; set; }
        public ILexemeFactory LexemeFactory { get; set; }

        public void AddMappedLexRule(ILexRule otherLexRule)
        {
            if(_mappedLexRules.Contains(otherLexRule))
                _mappedLexRules.Add(otherLexRule);
        }
    }
}
