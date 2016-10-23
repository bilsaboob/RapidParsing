using System;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Collections;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    public class DfaBuildContext
    {
        private Nfa _nfa;
        private ProcessOnceQueue<int, NfaClosure> _processQueue;
        private SortedSet<NfaState> _transitionStates;

        public DfaBuildContext(Nfa nfa)
        {
            _nfa = nfa;
            _processQueue = new ProcessOnceQueue<int, NfaClosure>();

            Init();
        }

        public Nfa Nfa { get { return _nfa; } }

        public NfaClosure StartNfaClosure { get; private set; }

        private void Init()
        {
            _processQueue = new ProcessOnceQueue<int, NfaClosure>();
            _transitionStates = new SortedSet<NfaState>();

            //Add the initial transition states
            var startStates = Nfa.Start.Closure();
            foreach (var startState in startStates)
                _transitionStates.Add(startState);

            StartNfaClosure = new NfaClosure(_transitionStates, Nfa.Start.Equals(Nfa.End));
            Enqueue(StartNfaClosure);
        }

        private bool Enqueue(NfaClosure closure)
        {
            return _processQueue.Enqueue(closure.GetHashCode(), closure);
        }

        public bool TryGetNextClosure(out NfaClosure closure)
        {
            closure = null;

            if (_processQueue.EnqueuedCount == 0)
                return false;

            closure = _processQueue.Dequeue();
            return closure != null;
        }

        public NfaClosure EnqueueOrGetExisting(NfaClosure closure)
        {
            var id = closure.GetHashCode();
            NfaClosure existingClosure;
            if(_processQueue.TryGetProcessed(id, out existingClosure))
            {
                return existingClosure;
            }

            _processQueue.Enqueue(id, closure);
            return closure;
        }
    }

    public class DfaBuilder
    {
        public DfaGraph Create(Nfa nfa)
        {
            var buildContext = new DfaBuildContext(nfa);
            var startDfaState = Build(buildContext, nfa);
            var g = new DfaGraph(startDfaState);
            return g;
        }

        public DfaState Build(DfaBuildContext c, Nfa nfa)
        {
            var end = nfa.End;

            //Keep processing closures until no more to process!
            NfaClosure currentNfaClosure;
            while (c.TryGetNextClosure(out currentNfaClosure))
            {
                var transitions = new NfaClsureTransitions();

                foreach (var nfaState in currentNfaClosure.NfaStates)
                {
                    ProcessNfaState(nfaState, transitions);
                }

                //Process the transitions by terminal
                foreach (var transitionsByTerminal in transitions.GetTransitionsByTerminal())
                {
                    var terminal = transitionsByTerminal.Terminal;
                    var terminalTransitions = transitionsByTerminal.Transitions;

                    //Get the closure for the transitions
                    var nextStateNfaClosure = Closure(terminalTransitions, end);
                    nextStateNfaClosure = c.EnqueueOrGetExisting(nextStateNfaClosure);

                    //Add transition to the DFA state
                    currentNfaClosure.State.AddTransition(new DfaTransition(terminal, nextStateNfaClosure.State));
                }
            }

            return c.StartNfaClosure.State;
        }

        private void ProcessNfaState(NfaState nfaState, NfaClsureTransitions transitions)
        {
            //Process the edge transitions, which need to bee progressed one step ahead
            foreach (var transition in nfaState.Transitions)
            {
                var terminalTransition = transition as TerminalNfaTransition;
                if(terminalTransition == null)
                    continue;
                
                transitions.AddTerminalTransition(terminalTransition);
            }
        }

        private static NfaClosure Closure(IEnumerable<NfaTransition> nfaTransitions, NfaState endState)
        {
            var transitions = new SortedSet<NfaState>();
            var isFinal = false;
            foreach (var nfaTransition in nfaTransitions)
            {
                var toState = nfaTransition.ToState;
                if(toState == null)
                    continue;

                var closure = toState.Closure();
                foreach (var nfaState in closure)
                {
                    if (nfaState.Equals(endState))
                        isFinal = true;

                    transitions.Add(nfaState);
                }
            }

            return new NfaClosure(transitions, isFinal);
        }
    }

    public class NfaClsureTransitions
    {
        private TransitionsCollection<ITerminal, NfaTransition> _terminalTransitions;

        public NfaClsureTransitions()
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

    public class NfaClosure
    {
        private int _hashCode;

        public NfaClosure(SortedSet<NfaState> nfaStates, bool isFinal)
        {
            NfaStates = nfaStates;

            State = new DfaState(isFinal);

            ComputeHashCode();
        }

        public DfaState State { get; private set; }

        //This is basically a Production path... just need to build it during the NFA building...
        public SortedSet<NfaState> NfaStates { get; private set; }

        private void ComputeHashCode()
        {
            _hashCode = HashCode.Compute(NfaStates);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
    
    public static class NfaToDfaExtensions
    {
        public static DfaGraph ToDfa(this Nfa nfa)
        {
            var dfaBuilder = new DfaBuilder();
            var g = dfaBuilder.Create(nfa);
            return g;
        }
    }
}
