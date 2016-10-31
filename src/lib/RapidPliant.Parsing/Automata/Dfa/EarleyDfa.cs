using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Automata.Nfa;
using RapidPliant.Common.Parsing;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Parsing.Automata.Dfa
{
    public interface IEarleyDfaGraph : IDfaGraph
    {
    }

    public class EarleyDfaGraph : DfaGraph, IEarleyDfaGraph
    {
        public EarleyDfaGraph(IDfaState startState) 
            : base(startState)
        {
        }
    }

    public interface IEarleyDfaState : IDfaState
    {
    }

    public class EarleyDfaState : DfaState, IEarleyDfaState
    {
        public override DfaGraph ToDfaGraph()
        {
            var graph = new EarleyDfaGraph(this);
            graph.EnsureCompiled();
            return graph;
        }
    }

    public interface IEarleyDfaTransition : IDfaTransition
    {
        int GrammarDefId { get; }
    }

    public class EarleyDfaTransition : DfaTransition, IEarleyDfaTransition
    {
        public EarleyDfaTransition(int grammarDefId, IEnumerable<INfaTransition> nfaTransitons, IEnumerable<INfaTransition> nfaFinalTransitions, DfaState toState)
            : base(grammarDefId, nfaTransitons, nfaFinalTransitions, toState)
        {
            GrammarDefId = grammarDefId;
        }

        public int GrammarDefId { get; protected set; }
    }
}
