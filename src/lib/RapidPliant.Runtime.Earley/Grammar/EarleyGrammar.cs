using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar;
using RapidPliant.Runtime.Earley.Automata.Earley;

namespace RapidPliant.Runtime.Earley.Grammar
{
    public interface IEarleyGrammar
    {
        IEarleyDfa EarleyDfa { get; set; }
    }

    public class EarleyGrammar
    {
        private int LocalLexRuleIndex { get; set; }
        private List<ILexRule> AllLexRules { get; set; }

        private IEarleyNfa EarleyNfa { get; set; }
        private IEarleyDfa EarleyDfa { get; set; }

        public EarleyGrammar(IGrammar grammar)
        {
            Grammar = grammar;
        }

        public IGrammar Grammar { get; private set; }

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
            var lexRules = Grammar.GetLexRules();
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

        private void AddLexRule(ILexRule lexRule)
        {
        }
    }
}
