using System.Collections.Generic;
using System.Linq;
using RapidPliant.Automata;
using RapidPliant.Collections;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    #region Nfa
    public class NfaGraph : Graph<Nfa, NfaState, NfaTransition, NfaGraphBuildContext>
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

        protected override void BuildState(NfaGraphBuildContext c, NfaState state)
        {
            //Build for the specified state
            state.Path = c.CurrentPath;
        }
    }

    public class NfaGraphBuildContext : GraphBuildContext<Nfa, NfaState, NfaTransition>
    {
        public NfaGraphBuildContext()
        {
        }

        public NfaPath CurrentPath { get; set; }

        public override void EnterTransition(NfaTransition transition)
        {
            CurrentPath = new NfaPath(CurrentPath, transition);

            base.EnterTransition(transition);
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
        private List<NfaState> _expandedTransitionStates;

        public NfaState()
        {
            _transitions = new List<NfaTransition>();
        }

        public NfaPath Path { get; set; }

        public IReadOnlyList<NfaTransition> Transitions
        {
            get { return _transitions; }
        }
        
        public void AddTransistion(NfaTransition transition)
        {
            if(!_transitions.Contains(transition))
                _transitions.Add(transition);
        }

        public IEnumerable<NfaState> GetExpandedTransitionStates()
        {
            if (_expandedTransitionStates != null)
                return _expandedTransitionStates;
            
            var queue = ReusableProcessOnceQueue<NfaState>.GetAndClear();
            
            //Start from this state
            queue.Enqueue(this);

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
                        queue.Enqueue(targetState);
                    }
                }
            }
            
            _expandedTransitionStates = queue.Processed.ToList();
            
            queue.ClearAndFree();

            return _expandedTransitionStates;
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

    public class NfaPath
    {
        private string _pathStr;

        public NfaPath(NfaPath prevPath, NfaTransition transition)
        {
            Prev = prevPath;
            Transition = transition;
        }

        public NfaPath Prev { get; private set; }
        public NfaTransition Transition { get; private set; }

        public override string ToString()
        {
            if (_pathStr != null)
                return _pathStr;

            var prevPath = "";

            if (Prev != null)
            {
                Prev.ToString();
            }

            _pathStr = string.Format("{0}->{1}", prevPath, Transition.ToState.ToString());

            return _pathStr;
        }
    }
}
