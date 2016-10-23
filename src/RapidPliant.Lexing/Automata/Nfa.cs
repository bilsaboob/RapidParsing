using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Collections;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Rule;
using RapidPliant.Common.Symbols;
using RapidPliant.Lexing.Pattern;

namespace RapidPliant.Lexing.Graph
{
    #region Nfa
    public class NfaGraph : Graph<Nfa, NfaState, NfaTransition>
    {
        public NfaGraph(Nfa nfa)
            : base(nfa, nfa.Start)
        {
        }

        public Nfa Nfa { get { return Root; } }

        protected override IEnumerable<NfaTransition> GetStateTransitions(NfaState state)
        {
            return state.Transitions;
        }

        protected override NfaState GetTransitionToState(NfaTransition transition)
        {
            return transition.ToState;
        }
    }

    public class Nfa
    {
        public Nfa(NfaState start, NfaState end)
        {
            Start = start;
            End = end;
        }

        public NfaState Start { get; private set; }
        public NfaState End { get; private set; }
    }
    #endregion

    #region State
    public class NfaState : GraphStateBase<NfaState>
    {
        private List<NfaTransition> _transitions;

        public NfaState()
        {
            _transitions = new List<NfaTransition>();
        }
        
        public IReadOnlyList<NfaTransition> Transitions
        {
            get { return _transitions; }
        }

        public void AddTransistion(NfaTransition transition)
        {
            if(!_transitions.Contains(transition))
                _transitions.Add(transition);
        }

        public IEnumerable<NfaState> Closure()
        {
            var queue = new ProcessOnceQueue<int, NfaState>();
            
            //Start from this state
            queue.Enqueue(Id, this);

            //Follow the null transitions and any "recursive" null transitions
            while (queue.EnqueuedCount > 0)
            {
                var state = queue.Dequeue();
                for (var t = 0; t < state.Transitions.Count; t++)
                {
                    var transition = state.Transitions[t];
                    if (transition.TransitionType == NfaTransitionType.Null)
                    {
                        var targetState = transition.ToState;
                        queue.Enqueue(targetState.Id, targetState);
                    }
                }
            }

            return queue.Processed;
        }
    }
    #endregion

    #region Transition
    public enum NfaTransitionType
    {
        Null,
        Edge
    }

    public class NfaTransition
    {
        public NfaTransition(NfaTransitionType type, NfaState target)
        {
            TransitionType = type;
            ToState = target;
        }

        public NfaState ToState { get; private set; }
        public NfaTransitionType TransitionType { get; private set; }
    }

    public abstract class SymbolNfaTransition : NfaTransition
    {
        public SymbolNfaTransition(ISymbol symbol, NfaState target)
            : base(NfaTransitionType.Edge, target)
        {
            Symbol = symbol;
        }

        public ISymbol Symbol { get; protected set; }
    }

    public class TerminalNfaTransition : SymbolNfaTransition
    {
        public TerminalNfaTransition(ITerminal terminal, NfaState target)
            : base(terminal, target)
        {
            Terminal = terminal;
        }

        public ITerminal Terminal { get; protected set; }
    }

    public class NullNfaTransition : NfaTransition
    {
        public NullNfaTransition(NfaState target)
            : base(NfaTransitionType.Null, target)
        {
        }
    }
    #endregion
}
