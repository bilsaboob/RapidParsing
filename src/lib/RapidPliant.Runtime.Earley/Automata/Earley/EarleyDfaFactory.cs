using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Runtime.Earley.Grammar;

namespace RapidPliant.Runtime.Earley.Automata.Earley
{
    public class EarleyDfaFactory
    {
        public void Create(IEarleyGrammar grammar)
        {
            var startRules = grammar.GetStartRules();

            //var productions = startRule.Productions;
        }
    }
}
