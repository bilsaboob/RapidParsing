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

    public static class NfaGrapExtensions
    {
        public static NfaGraph ToNfaGraph(this Nfa nfa)
        {
            var g = new NfaGraph(nfa);
            return g;
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

        public IExpr Expression { get; set; }

        public Nfa WithExpression(IExpr expression)
        {
            Expression = expression;
            return this;
        }
    }
    #endregion

    #region State
    public class NfaState : GraphStateBase<NfaState>
    {
        private List<NfaTransition> _transitions;
        private List<NfaState> _expandedTransitionStates;

        public NfaState()
        {
            _transitions = ReusableList<NfaTransition>.GetAndClear();
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

        public NfaState FromState { get; private set; }
        public NfaState ToState { get; private set; }
        public NfaTransitionType TransitionType { get; private set; }
        public IExpr Expression { get; private set; }

        public NfaTransition WithExpression(IExpr expression)
        {
            Expression = expression;
            return this;
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

        public NfaTransition ThisOrClonedWithFromState(NfaState fromState)
        {
            if (FromState != null)
                return this;

            var clone = Clone();
            clone.FromState = fromState;
            return clone;
        }

        public NfaTransition Clone()
        {
            var clone = CreateCloneInstance();
            CopyTo(clone);
            return clone;
        }

        protected virtual void CopyTo(NfaTransition other)
        {
            other.FromState = FromState;
            other.ToState = ToState;
            other.TransitionType = TransitionType;
            other.Expression = Expression;
        }

        protected virtual NfaTransition CreateCloneInstance()
        {
            return new NfaTransition(TransitionType, ToState);
        }
    }

    public abstract class SymbolNfaTransition : NfaTransition
    {
        public SymbolNfaTransition(ISymbol symbol, NfaState target)
            : base(NfaTransitionType.Edge, target)
        {
            Symbol = symbol;
        }

        public ISymbol Symbol { get; protected set; }

        protected override string ToTransitionSymbolString()
        {
            if (Symbol == null)
                return "";

            return Symbol.ToString();
        }

        protected override void CopyTo(NfaTransition other)
        {
            base.CopyTo(other);

            var symTransition = other as SymbolNfaTransition;
            if(symTransition == null)
                return;

            symTransition.Symbol = Symbol;
        }
    }

    public class TerminalNfaTransition : SymbolNfaTransition
    {
        public TerminalNfaTransition(ITerminal terminal, NfaState target)
            : base(terminal, target)
        {
            Terminal = terminal;
        }

        public ITerminal Terminal { get; protected set; }

        protected override NfaTransition CreateCloneInstance()
        {
            return new TerminalNfaTransition(Terminal, ToState);
        }

        protected override void CopyTo(NfaTransition other)
        {
            base.CopyTo(other);

            var termTransition = other as TerminalNfaTransition;
            if (termTransition == null)
                return;

            termTransition.Terminal = Terminal;
        }
    }

    public class NullNfaTransition : NfaTransition
    {
        public NullNfaTransition(NfaState target)
            : base(NfaTransitionType.Null, target)
        {
        }

        protected override string ToTransitionArrowString()
        {
            return "=>";
        }

        protected override NfaTransition CreateCloneInstance()
        {
            return new NullNfaTransition(ToState);
        }

        protected override void CopyTo(NfaTransition other)
        {
            base.CopyTo(other);
        }
    }
    #endregion

    public class NfaPath
    {
        public NfaPath(NfaPath prevPath, NfaTransition transition)
        {
            Prev = prevPath;
            Transition = transition;
        }

        public NfaPath Prev { get; private set; }
        public NfaTransition Transition { get; private set; }

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
}
