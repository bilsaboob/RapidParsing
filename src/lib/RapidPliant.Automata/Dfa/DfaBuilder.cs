using System;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Automata.Nfa;
using RapidPliant.Collections;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Automata.Dfa
{
    public interface IDfaBuilder
    {
        IDfaState Create(INfaGraph nfaGraph);
    }
    
    public abstract class DfaBuilder : IDfaBuilder, IDisposable
    {
        protected ProcessOnceQueue<NfaClosure> _processQueue;
        protected SortedSet<INfaState> _transitionStates;

        public DfaBuilder()
        {
            _processQueue = ReusableProcessOnceQueue<NfaClosure>.GetAndClear();
            _transitionStates = ReusableSortedSet<INfaState>.GetAndClear();
        }

        public IDfaState Create(INfaGraph nfaGraph)
        {
            nfaGraph.EnsureCompiled();

            var nfa = nfaGraph.Nfa;

            BeginBuild(nfa);

            var dfaState = Build();

            Nfa = null;

            return dfaState;
        }

        private void BeginBuild(INfa nfa)
        {
            Nfa = nfa;
            //Get the initial closure to start with!
            StartNfaClosure = Closure(nfa.Start);

            Enqueue(StartNfaClosure);
        }
        
        public IDfaState Build()
        {
            //Keep processing closures until no more to process!
            while (true)
            {
                var closure = GetNextClosureToProcess();
                if(closure == null)
                    break;

                var stateBuilder = CreateStateBuilder(closure);

                if (closure.HasStates)
                {
                    foreach (var nfaState in closure.States)
                    {
                        stateBuilder.BuildForNfaState(nfaState);
                    }
                }

                stateBuilder.Build();
            }
            
            return StartNfaClosure.DfaState;
        }

        protected abstract DfaStateBuilder CreateStateBuilder(NfaClosure closure);

        #region Helpers
        
        protected INfa Nfa { get; private set; }

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
            var closure = new NfaClosure(CreateDfaState());
            EvalClosureForState(closure, null, toState);
            return closure;
        }

        protected virtual DfaState CreateDfaState()
        {
            return new DfaState();
        }

        public NfaClosure Closure(IEnumerable<INfaTransition> nfaTransitions)
        {
            var closure = new NfaClosure(CreateDfaState());
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
        #endregion

        #region Dispose
        protected bool IsDisposed { get; set; }

        ~DfaBuilder()
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
            if (_processQueue != null)
            {
                _processQueue.ClearAndFree();
                _processQueue = null;
            }

            if (_transitionStates != null)
            {
                _transitionStates.ClearAndFree();
                _transitionStates = null;
            }
        }
        #endregion
    }

    public abstract class DfaStateBuilder : IDisposable
    {
        protected DfaBuilder _dfaBuilder;

        public DfaStateBuilder(DfaBuilder dfaBuilder, NfaClosure closure)
        {
            _dfaBuilder = dfaBuilder;

            ClosureTransitions = new NfaClosureTransitions();
            Closure = closure;
        }

        public NfaClosure Closure { get; set; }

        public NfaClosureTransitions ClosureTransitions { get; set; }

        public void BuildForNfaState(INfaState nfaState)
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

        public void Build()
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
                var closure = _dfaBuilder.Closure(nfaTransitions);

                //Only process each "location" once
                var nextClosure = _dfaBuilder.EnqueueOrGetExisting(closure);
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

        protected virtual void CollectCompletions(DfaTransition dfaTransition, IEnumerable<INfaTransition> nfaTransitions)
        {
            if (nfaTransitions == null)
                return;

            var completionsByExpression = ReusableDictionary<IExpr, DfaCompletion>.GetAndClear();

            //Collect the grouped dfa completions for the dfa transition - group by "Rule" / "Expresion"
            foreach (var nfaTransition in nfaTransitions)
            {
                var expr = nfaTransition.Expression;
                if (expr == null)
                    continue;

                DfaCompletion completion;
                if (!completionsByExpression.TryGetValue(expr, out completion))
                {
                    completion = new DfaCompletion(dfaTransition, expr);
                    dfaTransition.AddCompletion(completion);
                    completionsByExpression[expr] = completion;
                }

                completion.AddNfaTransition(nfaTransition);
            }

            completionsByExpression.ClearAndFree();
        }

        protected abstract IEnumerable<INfaTransitionsByValue> GetNfaTransitionsByValue();

        protected abstract IDfaTransition CreateDfaTransition(object transitionValue, IEnumerable<INfaTransition> nfaTransitions, HashSet<INfaTransition> finalNfaTransitions, DfaState toDfaState);

        #region Dispose
        protected bool IsDisposed { get; set; }

        ~DfaStateBuilder()
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
            if (ClosureTransitions != null)
            {
                ClosureTransitions.Dispose();
            }
        }
        #endregion
    }

    public class NfaClosureTransitions
    {
        private Dictionary<object, DfaTransitionsEntry> _nfaTransitionsByValue;

        public NfaClosureTransitions()
        {
            _nfaTransitionsByValue = new Dictionary<object, DfaTransitionsEntry>();
        }
        
        public DfaTransitionsEntry AddTransition(object transitionValue, INfaTransition valueNfaTransition)
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

        public IEnumerable<INfaTransitionsByValue> GetTransitionsByTransitionValues()
        {
            return _nfaTransitionsByValue.Select(p => new ClosureNfaTransitionsByValue(p.Value));
        }

        #region Dispose
        protected bool IsDisposed { get; set; }

        ~NfaClosureTransitions()
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
            if (_nfaTransitionsByValue != null)
            {
                _nfaTransitionsByValue.Clear();
                _nfaTransitionsByValue = null;
            }
        }
        #endregion

        public class DfaTransitionsEntry
        {
            private List<ITerminal> _terminals;
            private List<INfaTransition> _transitions;

            internal DfaTransitionsEntry()
            {
                _terminals = new List<ITerminal>();
                _transitions = new List<INfaTransition>();
            }

            public object Value { get; set; }

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

        private class ClosureNfaTransitionsByValue : INfaTransitionsByValue
        {
            private DfaTransitionsEntry _transitionEntry;

            public ClosureNfaTransitionsByValue(DfaTransitionsEntry transitionEntry)
            {
                _transitionEntry = transitionEntry;
            }

            public object TransitionValue { get { return _transitionEntry.Value; } }
            public IEnumerable<ITerminal> Terminals { get { return _transitionEntry.Terminals; } }
            public IEnumerable<INfaTransition> Transitions { get { return _transitionEntry.Transitions; } }
        }
    }

    public interface INfaTransitionsByValue
    {
        object TransitionValue { get; }
        IEnumerable<INfaTransition> Transitions { get; }
    }

    public class NfaClosure : IComparable<NfaClosure>, IComparable, IDisposable
    {
        private int _hashCode;
        
        public NfaClosure(DfaState dfaState)
        {
            DfaState = dfaState;
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

}
