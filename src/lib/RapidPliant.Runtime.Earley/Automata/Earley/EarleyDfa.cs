using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar;
using RapidPliant.Runtime.Earley.Grammar;
using RapidPliant.Runtime.Earley.Parsing;

namespace RapidPliant.Runtime.Earley.Automata.Earley
{
    public interface IEarleyDfa
    {
        IEarleyDfaState StartState { get; set; }
    }

    public interface IEarleyDfaState
    {
        IEarleyDfaTransitionsByToken TokenTransitions { get; }
        IEarleyDfaTransitionsByLexRules ScanTransitions { get; set; }
        IEarleyDfaTransitionsByRuleDefs RuleTransitions { get; set; }
        IEarleyDfaCompletion[] Completions { get; set; }
    }
    
    public interface IEarleyDfaTransition
    {
        IEarleyDfaState ToState { get; set; }
    }

    public interface IEarleyDfaTransitionsByToken
    {
        IEarleyDfaTransition Get(IToken token);
    }

    public interface IEarleyDfaTransitionsByLexRules
    {
        IEarleyLexDef[] AllLexDefs { get; set; }
    }

    public interface IEarleyDfaTransitionsByRuleDefs
    {
        IEarleyDfaTransition Get(IEarleyRuleDef ruleDef);
    }

    public interface IEarleyDfaCompletion
    {
        IEarleyRuleDef RuleDef { get; set; }
    }
}
