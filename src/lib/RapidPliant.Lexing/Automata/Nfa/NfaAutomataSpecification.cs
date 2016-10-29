using System.Collections.Generic;
using System.Linq;
using RapidPliant.Automata;
using RapidPliant.Collections;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    #region Automata specification
    public abstract class NfaAutomataSpecification<TAutomata, TNfaGraph, TNfaGraphBuildContext, TNfa, TNfaState, TNfaTransition, TNfaBuilder, TNfaBuildContext>
        where TAutomata : new()
        where TNfaGraph : NfaGraph<TNfa, TNfaState, TNfaTransition, TNfaGraphBuildContext>
        where TNfaGraphBuildContext : NfaGraphBuildContext<TNfa, TNfaState, TNfaTransition>, new()
        where TNfa : Nfa<TNfa, TNfaState, TNfaTransition>
        where TNfaState : NfaState<TNfaState, TNfaTransition>
        where TNfaTransition : NfaTransition<TNfaState, TNfaTransition>
        where TNfaBuilder : NfaBuilder<TNfa, TNfaState, TNfaTransition, TNfaBuildContext>
        where TNfaBuildContext : NfaBuildContext<TNfa, TNfaState, TNfaTransition>, new()
    {
        private static TAutomata _instance;

        public static TAutomata Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TAutomata();
                }
                return _instance;
            }
        }

        public abstract class NfaGraphBase : NfaGraph<TNfa, TNfaState, TNfaTransition, TNfaGraphBuildContext>
        {
            public NfaGraphBase(TNfa nfa) 
                : base(nfa)
            {
            }
        }

        public abstract class NfaGraphBuildContextBase : NfaGraphBuildContext<TNfa, TNfaState, TNfaTransition>
        {
        }

        public abstract class NfaBase : Nfa<TNfa, TNfaState, TNfaTransition>
        {
            public NfaBase()
            {
            }

            protected NfaBase(TNfaState startState, TNfaState endState)
                : base(startState, endState)
            {
            }
        }

        public abstract class NfaStateBase : NfaState<TNfaState, TNfaTransition>
        {
        }

        public abstract class NfaTransitionBase : NfaTransition<TNfaState, TNfaTransition>
        {
            public NfaTransitionBase()
            {
            }

            public NfaTransitionBase(TNfaState fromState, TNfaState toState)
                : base(fromState, toState)
            {
            }
        }

        public abstract class NfaBuildContextBase : NfaBuildContext<TNfa, TNfaState, TNfaTransition>
        {
        }

        public abstract class NfaBuilderBase : NfaBuilder<TNfa, TNfaState, TNfaTransition, TNfaBuildContext>
        {
        }
    }
    #endregion

    #region Nfa
    public interface INfaGraph : IStateGraph<INfa, INfaState>
    {
        INfa Nfa { get; }
    }

    public abstract class NfaGraph<TNfa, TNfaState, TNfaTransition, TNfaGraphBuildContext> : Graph<TNfa, TNfaState, TNfaTransition, TNfaGraphBuildContext>, INfaGraph
        where TNfa : Nfa<TNfa, TNfaState, TNfaTransition>
        where TNfaState : NfaState<TNfaState, TNfaTransition>
        where TNfaTransition : NfaTransition<TNfaState, TNfaTransition>
        where TNfaGraphBuildContext : NfaGraphBuildContext<TNfa, TNfaState, TNfaTransition>, new()
    {
        public NfaGraph(TNfa nfa)
            : base(nfa, nfa.Start)
        {
        }

        public TNfa Nfa { get { return Root; } }

        protected override IEnumerable<TNfaTransition> GetStateTransitions(TNfaState state)
        {
            return state.Transitions;
        }

        protected override TNfaState GetTransitionToState(TNfaTransition transition)
        {
            return transition.ToState;
        }

        protected override void BuildState(TNfaGraphBuildContext c, TNfaState state)
        {
            //Build for the specified state
            state.Path = c.CurrentPath;
        }

        INfa INfaGraph.Nfa { get { return base.Root; } }
        INfa IStateGraph<INfa, INfaState>.Root { get { return base.Root; } }
        INfaState IStateGraph<INfaState>.StartState { get { return base.StartState; } }
        IReadOnlyList<INfaState> IStateGraph<INfaState>.States { get { return base.States; } }
    }

    public abstract class NfaGraphBuildContext<TNfa, TNfaState, TNfaTransition> : GraphBuildContext<TNfa, TNfaState, TNfaTransition>
        where TNfa : Nfa<TNfa, TNfaState, TNfaTransition>
        where TNfaState : NfaState<TNfaState, TNfaTransition>
        where TNfaTransition : NfaTransition<TNfaState, TNfaTransition>
    {
        public NfaGraphBuildContext()
        {
        }

        public NfaPath<TNfaTransition> CurrentPath { get; set; }

        public override void EnterTransition(TNfaTransition transition)
        {
            CurrentPath = new NfaPath<TNfaTransition>(CurrentPath, transition);

            base.EnterTransition(transition);
        }
    }
    
    public interface INfa
    {
        INfaState Start { get; }
        INfaState End { get; }

        INfaGraph ToNfaGraph();
    }

    public abstract class Nfa<TNfa, TNfaState, TNfaTransition> : INfa
        where TNfa : Nfa<TNfa, TNfaState, TNfaTransition>
        where TNfaState : NfaState<TNfaState, TNfaTransition>
        where TNfaTransition : NfaTransition<TNfaState, TNfaTransition>
    {
        protected TNfa _this;

        protected Nfa()
        {
            _this = (TNfa)this;
        }

        protected Nfa(TNfaState startState, TNfaState endState)
        {
            _this = (TNfa)this;

            Start = startState;
            End = endState;
        }

        public TNfa Self { get { return _this; } }

        public TNfaState Start { get; protected set; }
        INfaState INfa.Start { get { return Start; } }

        public TNfaState End { get; protected set; }
        INfaState INfa.End { get { return End; } }

        public IExpr Expression { get; set; }

        public abstract INfaGraph ToNfaGraph();
    }
    #endregion

    #region State
    public interface INfaState
    {
        int Id { get; }
        IReadOnlyList<INfaTransition> Transitions { get; }
        bool AddTransition(INfaTransition transition);
        IEnumerable<INfaState> GetExpandedTransitionStates();
    }
    
    public abstract class NfaState<TNfaState, TNfaTransition> : GraphStateBase<TNfaState>, INfaState
        where TNfaState : NfaState<TNfaState, TNfaTransition>
        where TNfaTransition : NfaTransition<TNfaState, TNfaTransition>
    {
        protected TNfaState _this;

        protected List<TNfaTransition> _transitions;
        protected List<TNfaState> _expandedTransitionStates;

        public NfaState()
        {
            _this = (TNfaState)this;

            _transitions = new List<TNfaTransition>();
        }

        public TNfaState Self { get { return _this; } }

        public NfaPath<TNfaTransition> Path { get; set; }
        
        public IReadOnlyList<TNfaTransition> Transitions { get { return _transitions; } }
        IReadOnlyList<INfaTransition> INfaState.Transitions { get { return Transitions; } }
        
        public bool AddTransistion(TNfaTransition transition)
        {
            if (_transitions.Contains(transition))
                return false;

            _transitions.Add(transition);
            return true;
        }

        bool INfaState.AddTransition(INfaTransition transition)
        {
            return AddTransistion((TNfaTransition)transition);
        }
        
        public IEnumerable<TNfaState> GetExpandedTransitionStates()
        {
            if (_expandedTransitionStates != null)
                return _expandedTransitionStates;

            _expandedTransitionStates = GetExpandedTransitionStatesInternal();

            return _expandedTransitionStates;
        }

        IEnumerable<INfaState> INfaState.GetExpandedTransitionStates()
        {
            return GetExpandedTransitionStates();
        }

        public List<TNfaState> GetExpandedTransitionStatesInternal()
        {
            var queue = ReusableProcessOnceQueue<TNfaState>.GetAndClear();
            
            //Start from this state
            queue.Enqueue(Self);

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
                            queue.Enqueue(toState);
                        }
                    }
                }
            }

            _expandedTransitionStates = ReusableList<TNfaState>.GetAndClear(queue.Processed);
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

        public override void Dispose()
        {
            base.Dispose();

            if (_expandedTransitionStates != null)
            {
                _expandedTransitionStates.ClearAndFree();
                _expandedTransitionStates = null;
            }
        }
    }
    #endregion

    #region Transition
    public interface INfaTransition
    {
        INfaState FromState { get; }
        INfaState ToState { get; }

        IExpr Expression { get; }

        string ToTransitionString();
    }

    public interface INfaNullTransition : INfaTransition
    {
    }

    public abstract class NfaTransition<TNfaState, TNfaTransition> : INfaTransition, IGraphTransition
        where TNfaState : NfaState<TNfaState, TNfaTransition>
        where TNfaTransition : NfaTransition<TNfaState, TNfaTransition>
    {
        protected TNfaTransition _this;

        protected NfaTransition()
        {
            _this = (TNfaTransition)this;
        }

        protected NfaTransition(TNfaState fromState, TNfaState toState)
        {
            FromState = fromState;
            ToState = toState;
        }

        public TNfaTransition Self { get { return _this; } }
        
        public TNfaState FromState { get; protected set; }
        IGraphState IGraphTransition.FromState { get { return FromState; } }
        INfaState INfaTransition.FromState { get { return FromState; } }

        public TNfaState ToState { get; protected set; }

        void IGraphTransition.EnsureFromState(IGraphState fromState)
        {
            if (FromState == null)
                FromState = (TNfaState)fromState;
        }

        void IGraphTransition.EnsureToState(IGraphState toState)
        {
            if (ToState == null)
                ToState = (TNfaState)toState;
        }

        IGraphState IGraphTransition.ToState { get { return ToState; } }
        INfaState INfaTransition.ToState { get { return ToState; } }

        public IExpr Expression { get; set; }

        public TNfaTransition WithExpression(IExpr expression)
        {
            Expression = expression;
            return Self;
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
    }
    #endregion

    #region Path
    public class NfaPath<TNfaTransition>
        where TNfaTransition : INfaTransition
    {
        public NfaPath(NfaPath<TNfaTransition> prevPath, TNfaTransition transition)
        {
            Prev = prevPath;
            Transition = transition;
        }

        public NfaPath<TNfaTransition> Prev { get; private set; }
        public TNfaTransition Transition { get; private set; }

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
