using System;
using System.Collections.Generic;
using RapidPliant.Automata;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
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
        private List<DfaTransition> _transitions;
        
        public DfaState()
        {
            _transitions = ReusableList<DfaTransition>.GetAndClear();
        }
        
        public bool IsFinal { get; set; }

        public IReadOnlyList<DfaTransition> Transitions { get { return _transitions; } }

        public void AddTransition(DfaTransition transition)
        {
            _transitions.Add(transition);
        }

        public override void Dispose()
        {
            if (_transitions != null)
            {
                _transitions.ClearAndFree();
            }
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
