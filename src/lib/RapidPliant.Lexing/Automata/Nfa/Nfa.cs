using System;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Automata;
using RapidPliant.Collections;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    #region Nfa
    public interface INfaGraph : IStateGraph
    {
        new INfaState StartState { get; }
        new IReadOnlyList<INfaState> States { get; }
        INfa Nfa { get; }
    }

    public class NfaGraph : Graph<INfaState>, INfaGraph
    {
        public NfaGraph(INfa nfa)
            : base(nfa.Start)
        {
            Nfa = nfa;
        }

        public INfa Nfa { get; protected set; }
    }

    public interface INfa
    {
        INfaState Start { get; }
        INfaState End { get; }

        IExpr Expression { get; set; }

        INfaGraph ToNfaGraph();
    }

    public class Nfa : INfa
    {
        public Nfa()
        {
        }

        public Nfa(INfaState startState, INfaState endState)
        {
            Start = startState;
            End = endState;
        }
        
        public INfaState Start { get; protected set; }
        INfaState INfa.Start { get { return Start; } }

        public INfaState End { get; protected set; }
        INfaState INfa.End { get { return End; } }

        public IExpr Expression { get; set; }

        public virtual INfaGraph ToNfaGraph()
        {
            var graph = new NfaGraph(this);
            graph.EnsureCompiled();
            return graph;
        }
    }

    #endregion

    #region State
    public interface INfaState : IGraphState
    {
        new IReadOnlyList<INfaTransition> Transitions { get; }

        bool AddTransition(INfaTransition transition);

        IEnumerable<INfaState> GetExpandedTransitionStates();
    }
    
    public class NfaState : GraphStateBase, INfaState
    {
        protected UniqueList<NfaTransition> _transitions;
        protected List<NfaState> _expandedTransitionStates;

        public NfaState()
        {
            _transitions = new UniqueList<NfaTransition>();
        }

        public NfaPath Path { get; set; }
        
        public IReadOnlyList<INfaTransition> Transitions { get { return _transitions; } }
        IReadOnlyList<INfaTransition> INfaState.Transitions { get { return Transitions; } }
        IReadOnlyList<IGraphTransition> IGraphState.Transitions { get { return Transitions; } }

        public bool AddTransition(NfaTransition transition)
        {
            return _transitions.AddIfNotExists(transition);
        }

        bool INfaState.AddTransition(INfaTransition transition)
        {
            return AddTransition((NfaTransition) transition);
        }

        public IEnumerable<INfaState> GetExpandedTransitionStates()
        {
            if (_expandedTransitionStates != null)
                return _expandedTransitionStates;

            _expandedTransitionStates = GetExpandedTransitionStatesInternal();

            return _expandedTransitionStates;
        }

        public List<NfaState> GetExpandedTransitionStatesInternal()
        {
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
                    var nullTransition = transition as INfaNullTransition;
                    if (nullTransition != null)
                    {
                        var toState = transition.ToState;
                        if (toState != null)
                        {
                            queue.Enqueue((NfaState)toState);
                        }
                    }
                }
            }

            _expandedTransitionStates = ReusableList<NfaState>.GetAndClear(queue.Processed);
            queue.ClearAndFree();

            return _expandedTransitionStates;
        }

        public override string ToString()
        {
            if (Path == null)
            {
                return string.Format("{0}", Id);
            }
            else
            {
                return string.Format("{0}: {1}", Id, Path.ToString());
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (_expandedTransitionStates != null)
            {
                _expandedTransitionStates.Clear();
                _expandedTransitionStates = null;
            }

            if (_transitions == null)
            {
                _transitions.Clear();
                _transitions = null;
            }
        }
    }
    #endregion

    #region Transition
    public interface INfaTransition : IGraphTransition
    {
        new INfaState FromState { get; }
        new INfaState ToState { get; }

        IExpr Expression { get; set; }

        string ToTransitionString();
    }

    public interface INfaNullTransition : INfaTransition
    {
    }

    public abstract class NfaTransition : INfaTransition, IDisposable
    {
        protected NfaTransition()
        {
        }

        protected NfaTransition(INfaState fromState, INfaState toState)
        {
            FromState = fromState;
            ToState = toState;
        }

        public IExpr Expression { get; set; }

        public INfaState FromState { get; protected set; }
        IGraphState IGraphTransition.FromState { get { return FromState; } }
        INfaState INfaTransition.FromState { get { return FromState; } }
        public void EnsureFromState(IGraphState fromState)
        {
            if (FromState == null)
                FromState = (INfaState)fromState;
        }

        public INfaState ToState { get; protected set; }
        IGraphState IGraphTransition.ToState { get { return ToState; } }
        INfaState INfaTransition.ToState { get { return ToState; } }
        public void EnsureToState(IGraphState toState)
        {
            if (ToState == null)
                ToState = (INfaState)toState;
        }
        
        public string ToTransitionString()
        {
            var toStateStr = "";
            if (ToState == null)
            {
                toStateStr = "\u25CF";
            }
            else
            {
                toStateStr = ToState.Id.ToString();
            }

            var transitionArrowStr = ToTransitionArrowString();
            var symbolStr = ToTransitionSymbolString();
            if (string.IsNullOrEmpty(symbolStr))
            {
                return string.Format("{0}{1}", transitionArrowStr, toStateStr);
            }
            else
            {
                return string.Format("{0}{1}({2})", transitionArrowStr, toStateStr, symbolStr);
            }
        }

        protected virtual string ToTransitionArrowString()
        {
            return "->";
        }

        protected virtual string ToTransitionSymbolString()
        {
            return null;
        }

        #region Dispose
        protected bool IsDisposed { get; set; }

        ~NfaTransition()
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
        }
        #endregion
    }
    #endregion

    #region Path
    public class NfaPath
    {
        public NfaPath(NfaPath prevPath, INfaTransition transition)
        {
            Prev = prevPath;
            Transition = transition;
        }

        public NfaPath Prev { get; private set; }
        public INfaTransition Transition { get; private set; }

        public override string ToString()
        {
            var prevPath = "";

            if (Prev == null)
            {
                prevPath = "\u25CF";
            }
            else
            {
                prevPath = Prev.ToString();
            }

            var transStr = "";
            if (Transition != null)
            {
                transStr = Transition.ToTransitionString();
            }

            var pathStr = string.Format("{0}{1}", prevPath, transStr);
            return pathStr;
        }
    }
    #endregion
}
