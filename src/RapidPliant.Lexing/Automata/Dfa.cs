using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Common.Symbols;

namespace RapidPliant.Lexing.Graph
{
    public class DfaGraph : Graph<DfaState, DfaState, DfaTransition>
    {
        public DfaGraph(DfaState startState)
            : base(startState, startState)
        {
        }

        protected override DfaState GetTransitionToState(DfaTransition transition)
        {
            return transition.ToState;
        }

        protected override IEnumerable<DfaTransition> GetStateTransitions(DfaState state)
        {
            return state.Transitions;
        }
    }

    public class DfaState : GraphStateBase<DfaState>
    {
        private readonly List<DfaTransition> _transitions;
        
        public DfaState(bool isFinal)
        {
            IsFinal = isFinal;
            _transitions = new List<DfaTransition>();
        }
        
        public bool IsFinal { get; private set; }

        public IReadOnlyList<DfaTransition> Transitions { get { return _transitions; } }

        public void AddTransition(DfaTransition transition)
        {
            _transitions.Add(transition);
        }
    }

    public class DfaTransition
    {
        public DfaTransition(ITerminal terminal, DfaState toState)
        {
            Terminal = terminal;
            ToState = toState;
        }

        public DfaState ToState { get; private set; }
        public ITerminal Terminal { get; private set; }
    }
}
