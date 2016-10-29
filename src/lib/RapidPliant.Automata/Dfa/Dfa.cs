using System;
using System.Collections.Generic;
using RapidPliant.Automata.Nfa;
using RapidPliant.Collections;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Automata.Dfa
{
    public interface IDfaGraph : IStateGraph
    {
        new IDfaState StartState { get; }
        new IReadOnlyList<IDfaState> States { get; }
    }

    public class DfaGraph : Graph<IDfaState>, IDfaGraph
    {
        public DfaGraph(IDfaState startState)
            : base(startState)
        {
        }
    }

    public interface IDfaState : IGraphState
    {
        bool IsFinal { get; }

        new IReadOnlyList<IDfaTransition> Transitions { get; }

        bool AddTransition(IDfaTransition transition);

        DfaGraph ToDfaGraph();
    }

    public class DfaState : GraphStateBase<DfaState>, IDfaState
    {
        private UniqueList<DfaTransition> _transitions;
        
        public DfaState()
        {
            _transitions = new UniqueList<DfaTransition>();
        }
        
        public bool IsFinal { get; set; }

        public IReadOnlyList<DfaTransition> Transitions { get { return _transitions; } }
        IReadOnlyList<IDfaTransition> IDfaState.Transitions { get { return Transitions; } }
        IReadOnlyList<IGraphTransition> IGraphState.Transitions { get { return Transitions; } }

        public bool AddTransition(DfaTransition transition)
        {
            return _transitions.AddIfNotExists(transition);
        }
        public bool AddTransition(IDfaTransition transition)
        {
            return AddTransition((DfaTransition) transition);
        }

        public virtual DfaGraph ToDfaGraph()
        {
            var graph = new DfaGraph(this);
            graph.EnsureCompiled();
            return graph;
        }

        #region Dispose
        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (_transitions != null)
            {
                _transitions.Clear();
                _transitions = null;
            }
        }
        #endregion
    }

    public interface IDfaTransition : IGraphTransition
    {
        object TransitionValue { get; }

        new IDfaState FromState { get; }
        new IDfaState ToState { get; }

        IEnumerable<DfaCompletion> CompletionsByExpression { get; }
    }

    public class DfaTransition : IDfaTransition, IDisposable
    { 
        private HashSet<ITerminal> _terminals;
        private List<DfaCompletion> _completions;

        public DfaTransition(object transitionValue, IEnumerable<INfaTransition> nfaTransitons, IEnumerable<INfaTransition> nfaFinalTransitions, DfaState toState)
        {
            NfaTransitions = nfaTransitons;
            NfaFinalTransitions = nfaFinalTransitions;

            TransitionValue = transitionValue;
            ToState = toState;
        }

        public DfaState FromState { get; set; }
        IDfaState IDfaTransition.FromState { get { return FromState; } }
        IGraphState IGraphTransition.FromState { get { return FromState; } }
        void IGraphTransition.EnsureFromState(IGraphState fromState)
        {
            if (FromState == null)
                FromState = (DfaState)fromState;
        }

        public DfaState ToState { get; private set; }
        IDfaState IDfaTransition.ToState { get { return ToState; } }
        IGraphState IGraphTransition.ToState { get { return ToState; } }
        void IGraphTransition.EnsureToState(IGraphState toState)
        {
            if (ToState == null)
                ToState = (DfaState)toState;
        }

        public object TransitionValue { get; private set; }

        public IEnumerable<ITerminal> Terminals { get { return _terminals; } }

        public IEnumerable<INfaTransition> NfaTransitions { get; private set; }
        public IEnumerable<INfaTransition> NfaFinalTransitions { get; private set; }

        public IEnumerable<DfaCompletion> CompletionsByExpression { get { return _completions; } }
        
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

        #region Dispose
        protected bool IsDisposed { get; set; }

        ~DfaTransition()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(false);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool isDisposing)
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
        #endregion
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
        
        #region Dispose
        protected bool IsDisposed { get; set; }

        ~DfaCompletion()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(false);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_nfaTransitions != null)
            {
                _nfaTransitions.Clear();
                _nfaTransitions = null;
            }
        }
        #endregion
    }
}
