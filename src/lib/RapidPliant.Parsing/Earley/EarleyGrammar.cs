using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Automata.Nfa;
using RapidPliant.Common.Expression;
using RapidPliant.Grammar;
using RapidPliant.Parsing.Automata.Dfa;
using RapidPliant.Parsing.Automata.Nfa;

namespace RapidPliant.Parsing.Earley
{
    public class EarleyGrammar
    {
        private IEarleyNfa _nfa;
        private IEarleyNfaGraph _nfaGraph;

        private IEarleyDfaState _dfa;
        private IEarleyDfaGraph _dfaGraph;

        private bool _hasBuilt;

        public EarleyGrammar(IGrammarModel grammarModel)
        {
            GrammarModel = grammarModel;
        }

        public IGrammarModel GrammarModel { get; protected set; }

        public INfaGraph EarleyNfa { get { return _nfaGraph; } }

        public IDfaGraph EarleyDfa { get { return _dfaGraph; } }
        
        public void Build()
        {
            //Make sure the grammar has been built
            GrammarModel.EnsureBuild();

            //Build the earley dfa!
            var startRules = GrammarModel.GetStartRules().ToList();
            if(startRules.Count == 0)
                throw new Exception("No start rules defined for grammar!");

            //Build automata for the start expressions
            BuildAutomata(startRules.Select(r=>r.Expression));

            _hasBuilt = true;
        }

        private void BuildAutomata(IEnumerable<IExpr> expressions)
        {
            //Build the nfa
            _nfa = (IEarleyNfa)EarleyNfaAutomata.BuildNfa(expressions);
            _nfaGraph = (IEarleyNfaGraph)_nfa.ToNfaGraph();

            //Build the dfa
            _dfa = (IEarleyDfaState) EarleyDfaAutomata.BuildDfa(_nfaGraph);
            _dfaGraph = (IEarleyDfaGraph)_dfa.ToDfaGraph();
        }

        public void EnsureBuild()
        {
            if(!_hasBuilt)
                Build();
        }
    }
}
