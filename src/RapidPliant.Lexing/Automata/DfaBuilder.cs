using System;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Collections;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    public class DfaBuildContext : IDisposable
    {
        private Nfa _nfa;
        private ProcessOnceQueue<NfaClosure> _processQueue;
        private SortedSet<NfaState> _transitionStates;

        public DfaBuildContext(Nfa nfa)
        {
            _nfa = nfa;

            _processQueue = ReusableProcessOnceQueue<NfaClosure>.GetAndClear();
            _transitionStates = ReusableSortedSet<NfaState>.GetAndClear();
        }

        public Nfa Nfa { get { return _nfa; } }

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

        public NfaClosure Closure(IEnumerable<NfaTransition> nfaTransitions)
        {
            var closure = new NfaClosure();
            foreach (var nfaTransition in nfaTransitions)
            {
                var toState = nfaTransition.ToState;
                if (toState == null)
                    continue;

                //Expand the transitions
                var expandedStates = toState.GetExpandedTransitionStates();
                foreach (var nfaState in expandedStates)
                {
                    var isFinalState = nfaState.Equals(Nfa.End);
                    closure.AddState(nfaState, isFinalState);
                }
            }

            return closure;
        }

        public void Dispose()
        {
            _processQueue.ClearAndFree();
            _transitionStates.ClearAndFree();
        }
    }

    public class DfaBuilder
    {
        public DfaBuilder()
        {
        }

        public DfaState Create(Nfa nfa)
        {
            using (var buildContext = BeginBuild(nfa))
            {
                return Build(buildContext);
            }
        }

        private DfaBuildContext BeginBuild(Nfa nfa)
        {
            var c = new DfaBuildContext(nfa);
            //Get the initial closure to start with!
            c.StartNfaClosure = c.Closure(new[]{new NullNfaTransition(nfa.Start)});
            c.Enqueue(c.StartNfaClosure);
            return c;
        }

        public DfaState Build(DfaBuildContext c)
        {
            //Keep processing closures until no more to process!
            while (true)
            {
                var closure = c.GetNextClosureToProcess();
                if(closure == null)
                    break;

                var stateBuilder = new DfaStateBuilder(closure);

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
    }

    public class DfaStateBuilder
    {
        public DfaStateBuilder(NfaClosure closure)
        {
            Transitions = new NfaClosureTransitions();
            Closure = closure;
        }

        public NfaClosure Closure { get; set; }

        public NfaClosureTransitions Transitions { get; set; }

        public void BuildForNfaState(DfaBuildContext c, NfaState nfaState)
        {
            //We will process the "from nfa state" to be part of this DFA state... basically we need to include the outgoing terminal transitions!
            foreach (var transition in nfaState.Transitions)
            {
                var terminalTransition = transition as TerminalNfaTransition;
                if (terminalTransition == null)
                    continue;

                Transitions.AddTerminalTransition(terminalTransition);
            }
        }

        public void Build(DfaBuildContext c)
        {
            var state = Closure.DfaState;

            //Process the transitions by unique symbol / terminal
            foreach (var transitionsByTerminal in Transitions.GetTransitionsByTerminal())
            {
                var terminal = transitionsByTerminal.Terminal;
                var terminalTransitions = transitionsByTerminal.Transitions;
                
                //Get the closure for the transitions - basically expand the transitions one step ahead, following until a "terminal transition" is reached...
                //We are basically expanding the "nfa path tree"
                var closure = c.Closure(terminalTransitions);

                //Only process each "location" once
                var nextClosure = c.EnqueueOrGetExisting(closure);
                if (nextClosure != closure)
                {
                    //We have received an existing... dispose the other...
                    closure.Dispose();
                }

                //Add transition to the next DFA state
                state.AddTransition(new DfaTransition(terminal, nextClosure.DfaState));
            }
        }
    }

    public class NfaClosureTransitions
    {
        private TransitionsCollection<ITerminal, NfaTransition> _terminalTransitions;

        public NfaClosureTransitions()
        {
            _terminalTransitions = new TransitionsCollection<ITerminal, NfaTransition>();
        }

        public void AddTerminalTransition(TerminalNfaTransition terminalTransition)
        {
            var terminal = terminalTransition.Terminal;
            _terminalTransitions.AddTransition(terminal, terminalTransition);
        }

        public IEnumerable<TerminalTransitionsCollection> GetTransitionsByTerminal()
        {
            return _terminalTransitions.GetTransitionsByKey().Select(p => new TerminalTransitionsCollection(p.Key, p.Value));
        }

        private class TransitionsCollection<TKey, TTransition>
            where TTransition : NfaTransition
        {
            private Dictionary<TKey, SortedSet<TransitionEntry>> _transitions;

            public TransitionsCollection()
            {
                _transitions = new Dictionary<TKey, SortedSet<TransitionEntry>>();
            }

            public bool AddTransition(TKey key, TTransition transition)
            {
                SortedSet<TransitionEntry> transitions;
                if (!_transitions.TryGetValue(key, out transitions))
                {
                    transitions = new SortedSet<TransitionEntry>();
                    _transitions[key] = transitions;
                }

                return transitions.Add(new TransitionEntry(transition));
            }

            public class TransitionEntry : IComparable<TransitionEntry>, IComparable
            {
                public TransitionEntry(TTransition transition)
                {
                    Transition = transition;
                    ToState = Transition.ToState;
                }

                public TTransition Transition { get; set; }

                public NfaState ToState { get; set; }

                public int CompareTo(object obj)
                {
                    if (obj == null)
                        throw new NullReferenceException();

                    var entry = obj as TransitionEntry;
                    if (entry == null)
                        throw new ArgumentException("parameter must be a TransitionEntry", nameof(obj));

                    return CompareTo(entry);
                }

                public int CompareTo(TransitionEntry other)
                {
                    return ToState.CompareTo(other.ToState);
                }
            }

            public IEnumerable<KeyValuePair<TKey, IEnumerable<TTransition>>> GetTransitionsByKey()
            {
                foreach (var p in _transitions)
                {
                    var key = p.Key;
                    var transitions = p.Value.Select(e => e.Transition);
                    yield return new KeyValuePair<TKey, IEnumerable<TTransition>>(key, transitions);
                }
            }
        }
    }

    public class TerminalTransitionsCollection
    {
        public TerminalTransitionsCollection(ITerminal terminal, IEnumerable<NfaTransition> transitions)
        {
            Terminal = terminal;
            Transitions = transitions;
        }

        public ITerminal Terminal { get; set; }
        public IEnumerable<NfaTransition> Transitions { get; set; }
    }

    public class NfaClosure : IComparable<NfaClosure>, IComparable, IDisposable
    {
        private int _hashCode;
        
        public NfaClosure()
        {
            DfaState = new DfaState();
        }

        public DfaState DfaState { get; set; }

        public bool HasFinalStates { get; private set; }
        public HashSet<NfaState> FinalStates { get; private set; }
        
        public bool HasStates { get; private set; }
        public SortedSet<NfaState> States { get; private set; }

        public bool AddState(NfaState state, bool isFinal)
        {
            if (isFinal)
            {
                if (FinalStates == null)
                {
                    FinalStates = ReusableHashSet<NfaState>.GetAndClear();
                    HasFinalStates = true;
                    DfaState.IsFinal = true;
                }
                FinalStates.Add(state);
            }

            if (States == null)
            {
                States = ReusableSortedSet<NfaState>.GetAndClear();
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

            if (FinalStates != null)
            {
                HasFinalStates = false;
                FinalStates.ClearAndFree();
                FinalStates = null;
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
