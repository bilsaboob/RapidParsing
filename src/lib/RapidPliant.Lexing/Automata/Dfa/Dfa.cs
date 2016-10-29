using System;
using System.Collections.Generic;
using RapidPliant.Automata;
using RapidPliant.Common.Expression;
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

    public class DfaTransition : IDisposable, IGraphTransition
    {
        private HashSet<ITerminal> _terminals;
        private List<DfaCompletion> _completions;

        public DfaTransition(Interval interval, IEnumerable<INfaTransition> nfaTransitons, IEnumerable<INfaTransition> nfaFinalTransitions, DfaState toState)
        {
            NfaTransitions = nfaTransitons;
            NfaFinalTransitions = nfaFinalTransitions;

            Interval = interval;
            ToState = toState;
        }

        public DfaState FromState { get; set; }
        IGraphState IGraphTransition.FromState { get { return FromState; } }
        void IGraphTransition.EnsureFromState(IGraphState fromState)
        {
            if (FromState == null)
                FromState = (DfaState)fromState;
        }

        public DfaState ToState { get; private set; }
        IGraphState IGraphTransition.ToState { get { return ToState; } }
        void IGraphTransition.EnsureToState(IGraphState toState)
        {
            if (ToState == null)
                ToState = (DfaState)toState;
        }

        public Interval Interval { get; private set; }

        public IEnumerable<ITerminal> Terminals { get { return _terminals; } }

        public IEnumerable<INfaTransition> NfaTransitions { get; private set; }
        public IEnumerable<INfaTransition> NfaFinalTransitions { get; private set; }

        public IEnumerable<DfaCompletion> CompletionsByExpression { get { return _completions; } }

        public void Dispose()
        {
            if (_terminals != null)
            {
                _terminals.Clear();
                _terminals = null;
            }

            if (_completions != null)
            {
                _completions.Clear();
                _completions = null;
            }
        }

        public void AddCompletion(DfaCompletion completion)
        {
            if (_completions == null)
            {
                _completions = new List<DfaCompletion>();
            }

            _completions.Add(completion);
        }

        public void AddTerminal(ITerminal terminal)
        {
            if (_terminals == null)
            {
                _terminals = new HashSet<ITerminal>();
            }

            _terminals.Add(terminal);
        }
    }

    public class DfaCompletion : IDisposable
    {
        private HashSet<INfaTransition> _nfaTransitions;

        public DfaCompletion(DfaTransition dfaTransition, IExpr expr)
        {
            DfaTransition = dfaTransition;
            Expression = expr;

            _nfaTransitions = ReusableHashSet<INfaTransition>.GetAndClear();
        }
        
        public DfaTransition DfaTransition { get; private set; }
        public IExpr Expression { get; private set; }
        public IEnumerable<INfaTransition> NfaTransitions { get { return _nfaTransitions; } }

        public void AddNfaTransition(INfaTransition nfaTransition)
        {
            _nfaTransitions.Add(nfaTransition);
        }

        public void Dispose()
        {
            if (_nfaTransitions != null)
            {
                _nfaTransitions.ClearAndFree();
                _nfaTransitions = null;
            }
        }
    }
}
