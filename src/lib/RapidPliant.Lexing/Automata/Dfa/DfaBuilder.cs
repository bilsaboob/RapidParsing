using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Collections;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    public class DfaBuildContext : IDisposable
    {
        private INfa _nfa;
        private ProcessOnceQueue<NfaClosure> _processQueue;
        private SortedSet<INfaState> _transitionStates;

        public DfaBuildContext(INfa nfa)
        {
            _nfa = nfa;

            _processQueue = ReusableProcessOnceQueue<NfaClosure>.GetAndClear();
            _transitionStates = ReusableSortedSet<INfaState>.GetAndClear();
        }

        public INfa Nfa { get { return _nfa; } }

        public NfaClosure StartNfaClosure { get; set; }
        
        public NfaClosure GetNextClosureToProcess()
        {
            if (_processQueue.Count == 0)
                return null;

            return _processQueue.Dequeue();
        }

        public bool Enqueue(NfaClosure closure)
        {
            return _processQueue.Enqueue(closure);
        }

        public NfaClosure EnqueueOrGetExisting(NfaClosure closure)
        {
            return _processQueue.EnqueueOrGetExisting(closure);
        }

        public NfaClosure Closure(INfaState toState)
        {
            var closure = new NfaClosure();
            EvalClosureForState(closure, null, toState);
            return closure;
        }

        public NfaClosure Closure(IEnumerable<INfaTransition> nfaTransitions)
        {
            var closure = new NfaClosure();
            foreach (var nfaTransition in nfaTransitions)
            {
                var toState = nfaTransition.ToState;
                EvalClosureForState(closure, nfaTransition, toState);
            }

            return closure;
        }

        private void EvalClosureForState(NfaClosure closure, INfaTransition nfaTransition, INfaState toState)
        {
            if (toState == null)
                return;

            //Expand the transitions
            var expandedStates = toState.GetExpandedTransitionStates();
            foreach (var nfaState in expandedStates)
            {
                var isFinalState = nfaState.Equals(Nfa.End);
                if (isFinalState)
                {
                    closure.DfaState.IsFinal = true;
                    if (nfaTransition != null)
                    {
                        closure.AddFinalTransition(nfaTransition);
                    }
                }

                closure.AddState(nfaState);
            }
        }

        public void Dispose()
        {
            _processQueue.ClearAndFree();
            _transitionStates.ClearAndFree();
        }
    }

    public abstract class DfaBuilder<TTransitionValue, TStateBuilder, TDfaBuildContext>
        where TStateBuilder : DfaStateBuilder<TTransitionValue, TDfaBuildContext>
        where TDfaBuildContext : DfaBuildContext
    {
        public DfaBuilder()
        {
        }

        public DfaState Create(INfaGraph nfaGraph)
        {
            nfaGraph.EnsureCompiled();

            var nfa = nfaGraph.Nfa;

            using (var buildContext = BeginBuild(nfa))
            {
                return Build(buildContext);
            }
        }

        private TDfaBuildContext BeginBuild(INfa nfa)
        {
            var c = CreateBuildContext(nfa);
            //Get the initial closure to start with!
            c.StartNfaClosure = c.Closure(nfa.Start);

            c.Enqueue(c.StartNfaClosure);
            return c;
        }

        protected abstract TDfaBuildContext CreateBuildContext(INfa nfa);

        public DfaState Build(TDfaBuildContext c)
        {
            //Keep processing closures until no more to process!
            while (true)
            {
                var closure = c.GetNextClosureToProcess();
                if(closure == null)
                    break;

                var stateBuilder = CreateStateBuilder(closure);

                if (closure.HasStates)
                {
                    foreach (var nfaState in closure.States)
                    {
                        stateBuilder.BuildForNfaState(c, nfaState);
                    }
                }

                stateBuilder.Build(c);
            }
            
            return c.StartNfaClosure.DfaState;
        }

        protected abstract TStateBuilder CreateStateBuilder(NfaClosure closure);
    }

    public abstract class DfaStateBuilder<TTransitionValue, TDfaBuildContext>
        where TDfaBuildContext : DfaBuildContext
    {
        public DfaStateBuilder(NfaClosure closure)
        {
            ClosureTransitions = new NfaClosureTransitions<TTransitionValue>();
            Closure = closure;
        }

        public NfaClosure Closure { get; set; }

        public NfaClosureTransitions<TTransitionValue> ClosureTransitions { get; set; }

        public void BuildForNfaState(TDfaBuildContext c, INfaState nfaState)
        {
            var transitions = nfaState.Transitions;
            if(transitions == null)
                return;

            //We will process the "from nfa state" to be part of this DFA state... basically we need to include the outgoing terminal transitions!
            foreach (var transition in nfaState.Transitions)
            {
                //Build for each transition!
                BuildFornNfaTransition(transition);
            }
        }

        protected abstract void BuildFornNfaTransition(INfaTransition transition);

        public void Build(TDfaBuildContext c)
        {
            var dfaState = Closure.DfaState;

            //Process the transitions by unique symbol / terminal
            var nfaTransitionsByValueCollections = GetNfaTransitionsByValue();
            
            foreach (var nfaTransitionsByValue in nfaTransitionsByValueCollections)
            {
                //Get the transitions!
                var transitionValue = nfaTransitionsByValue.TransitionValue;
                var nfaTransitions = nfaTransitionsByValue.Transitions;

                //Get the closure for the transitions - basically expand the transitions one step ahead, following until a "terminal transition" is reached...
                //We are basically expanding the "nfa path tree"
                var closure = c.Closure(nfaTransitions);

                //Only process each "location" once
                var nextClosure = c.EnqueueOrGetExisting(closure);
                if (nextClosure != closure)
                {
                    //We have received an existing... dispose the other...
                    closure.Dispose();
                }
                
                //Add transition to the next DFA state
                var dfaTransition = CreateDfaTransition(transitionValue, nfaTransitions, nextClosure.FinalTransitions, nextClosure.DfaState);
                dfaState.AddTransition(dfaTransition);
            }
        }
        
        protected abstract IEnumerable<INfaTransitionsByValue<TTransitionValue>> GetNfaTransitionsByValue();

        protected abstract DfaTransition CreateDfaTransition(TTransitionValue transitionValue, IEnumerable<INfaTransition> nfaTransitions, HashSet<INfaTransition> finalNfaTransitions, DfaState toDfaState);
    }

    public class NfaClosureTransitions<TTransitionValue>
    {
        private Dictionary<TTransitionValue, DfaTransitionsEntry> _nfaTransitionsByValue;

        public NfaClosureTransitions()
        {
            _nfaTransitionsByValue = new Dictionary<TTransitionValue, DfaTransitionsEntry>();
        }
        
        public DfaTransitionsEntry AddTransition(TTransitionValue transitionValue, INfaTransition valueNfaTransition)
        {
            DfaTransitionsEntry transitionsForIntervalEntry;
            if (!_nfaTransitionsByValue.TryGetValue(transitionValue, out transitionsForIntervalEntry))
            {
                transitionsForIntervalEntry = new DfaTransitionsEntry();
                transitionsForIntervalEntry.Value = transitionValue;
                _nfaTransitionsByValue[transitionValue] = transitionsForIntervalEntry;
            }

            //Add the transition and terminal for the interval!
            transitionsForIntervalEntry.AddTransition(valueNfaTransition);
            return transitionsForIntervalEntry;
        }

        public IEnumerable<INfaTransitionsByValue<TTransitionValue>> GetTransitionsByTerminalIntervals()
        {
            return _nfaTransitionsByValue.Select(p => new ClosureNfaTransitionsByValue(p.Value));
        }

        public class DfaTransitionsEntry
        {
            private List<ITerminal> _terminals;
            private List<INfaTransition> _transitions;

            internal DfaTransitionsEntry()
            {
                _terminals = new List<ITerminal>();
                _transitions = new List<INfaTransition>();
            }

            public TTransitionValue Value { get; set; }

            public IReadOnlyList<ITerminal> Terminals { get { return _terminals; } }

            public IReadOnlyList<INfaTransition> Transitions { get { return _transitions; } }

            public void AddTerminal(ITerminal terminal)
            {
                _terminals.Add(terminal);
            }

            public void AddTransition(INfaTransition transition)
            {
                _transitions.Add(transition);
            }
        }

        private class ClosureNfaTransitionsByValue : INfaTransitionsByValue<TTransitionValue>
        {
            private DfaTransitionsEntry _transitionEntry;

            public ClosureNfaTransitionsByValue(DfaTransitionsEntry transitionEntry)
            {
                _transitionEntry = transitionEntry;
            }

            public TTransitionValue TransitionValue { get { return _transitionEntry.Value; } }
            public IEnumerable<ITerminal> Terminals { get { return _transitionEntry.Terminals; } }
            public IEnumerable<INfaTransition> Transitions { get { return _transitionEntry.Transitions; } }
        }
    }
    
    public interface INfaTransitionsByValue<TTransitionValue>
    {
        TTransitionValue TransitionValue { get; }
        IEnumerable<INfaTransition> Transitions { get; }
    }

    public class NfaClosure : IComparable<NfaClosure>, IComparable, IDisposable
    {
        private int _hashCode;
        
        public NfaClosure()
        {
            DfaState = new DfaState();
        }

        public DfaState DfaState { get; set; }
        
        public bool HasStates { get; private set; }
        public SortedSet<INfaState> States { get; private set; }

        public bool HasFinalTransitions { get; private set; }
        public HashSet<INfaTransition> FinalTransitions { get; private set; }

        public bool AddState(INfaState state)
        {
            if (States == null)
            {
                States = ReusableSortedSet<INfaState>.GetAndClear();
                HasStates = true;
            }

            var added = States.Add(state);
            
            if (added)
            {
                //Reset the hashcode if has computed
                _hashCode = 0;
            }

            return added;
        }

        public void AddFinalTransition(INfaTransition nfaTransition)
        {
            if (FinalTransitions == null)
            {
                FinalTransitions = ReusableHashSet<INfaTransition>.GetAndClear();
                HasFinalTransitions = true;
            }

            FinalTransitions.Add(nfaTransition);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            var nfaClosure = obj as NfaClosure;
            if (nfaClosure == null)
                throw new ArgumentException("instance of NfaClosure expected.", nameof(obj));

            return CompareTo(nfaClosure);
        }

        public int CompareTo(NfaClosure other)
        {
            return GetHashCode().CompareTo(other.GetHashCode());
        }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(States);
        }

        public override int GetHashCode()
        {
            if (_hashCode == 0)
                ComputeHashCode();

            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj == this)
                return true;

            var nfaClosure = obj as NfaClosure;
            if (nfaClosure == null)
                return false;

            return States.EqualsSequence(nfaClosure.States);
        }

        public void Dispose()
        {
            if (States != null)
            {
                HasStates = false;
                States.ClearAndFree();
                States = null;
            }
        }
    }
    
    public static class DfaExtensions
    {
        public static DfaGraph ToDfaGraph(this DfaState startState)
        {
            var g = new DfaGraph(startState);
            return g;
        }
    }
}
