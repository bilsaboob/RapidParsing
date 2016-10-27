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
                    if (isFinalState)
                    {
                        closure.DfaState.IsFinal = true;
                        closure.AddFinalTransition(nfaTransition);
                    }
                    
                    closure.AddState(nfaState);
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
            if (!nfa.Start.IsValid)
                nfa.ToNfaGraph();

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

                //Here we must take into account the different char intervals a terminal can have!
                Transitions.AddTerminalTransition((TerminalNfaTransition)terminalTransition.ThisOrClonedWithFromState(nfaState));
            }
        }

        public void Build(DfaBuildContext c)
        {
            var state = Closure.DfaState;

            //Process the transitions by unique symbol / terminal
            var transitions = Transitions.GetTransitionsByTerminalIntervals();

            //We need to split the terminal transitions into unique non overlapping intervals
            var nonOverlappingIntervalTransitions = new NonOverlappingIntervalSet<NfaTransition>();
            foreach (var transitionsByTerminalIntervals in transitions)
            {
                var interval = transitionsByTerminalIntervals.Interval;
                var nfaTransitions = transitionsByTerminalIntervals.Transitions;

                //Add the interval, which will split into non overlapping intervals, but keep the association of the nfa transitions for each of the splits
                nonOverlappingIntervalTransitions.AddInterval(interval, nfaTransitions);
            }

            //Now lets iterate each of the non overlapping intervall transitions
            foreach (var transitionsByTerminalIntervals in nonOverlappingIntervalTransitions)
            {
                var terminalTransitions = transitionsByTerminalIntervals.AssociatedItems;
                
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
                
                var interval = transitionsByTerminalIntervals.Interval;

                //Add transition to the next DFA state
                state.AddTransition(new DfaTransition(interval, terminalTransitions, nextClosure.FinalTransitions, nextClosure.DfaState));
            }
        }
    }

    public class NfaClosureTransitions
    {
        private Dictionary<Interval, TransitionsEntry> _terminalIntervalTransitions;

        public NfaClosureTransitions()
        {
            _terminalIntervalTransitions = new Dictionary<Interval, TransitionsEntry>();
        }

        public void AddTerminalTransition(TerminalNfaTransition terminalTransition)
        {
            //Instead of storing transitions per terminal - we must store the transitions per character range part of the terminal
            var terminal = terminalTransition.Terminal;

            var intervals = terminal.GetIntervals();
            foreach (var interval in intervals)
            {
                TransitionsEntry transitionsForIntervalEntry;
                if (!_terminalIntervalTransitions.TryGetValue(interval, out transitionsForIntervalEntry))
                {
                    transitionsForIntervalEntry = new TransitionsEntry();
                    transitionsForIntervalEntry.Interval = interval;
                    _terminalIntervalTransitions[interval] = transitionsForIntervalEntry;
                }

                //Add the transition and terminal for the interval!
                transitionsForIntervalEntry.AddTerminal(terminal);
                transitionsForIntervalEntry.AddTransition(terminalTransition);
            }
            
        }

        public IEnumerable<TerminalTransitionsCollection> GetTransitionsByTerminalIntervals()
        {
            return _terminalIntervalTransitions.Select(p => new TerminalTransitionsCollection(p.Value));
        }

        public class TransitionsEntry
        {
            private List<ITerminal> _terminals;
            private List<NfaTransition> _transitions;

            internal TransitionsEntry()
            {
                _terminals = new List<ITerminal>();
                _transitions = new List<NfaTransition>();
            }

            public Interval Interval { get; set; }

            public IReadOnlyList<ITerminal> Terminals { get { return _terminals; } }

            public IReadOnlyList<NfaTransition> Transitions { get { return _transitions; } }

            public void AddTerminal(ITerminal terminal)
            {
                _terminals.Add(terminal);
            }

            public void AddTransition(NfaTransition transition)
            {
                _transitions.Add(transition);
            }
        }

        public class TerminalTransitionsCollection
        {
            private TransitionsEntry _transitionEntry;

            public TerminalTransitionsCollection(TransitionsEntry transitionEntry)
            {
                _transitionEntry = transitionEntry;
            }

            public Interval Interval { get { return _transitionEntry.Interval; } }
            public IEnumerable<ITerminal> Terminals { get { return _transitionEntry.Terminals; } }
            public IEnumerable<NfaTransition> Transitions { get { return _transitionEntry.Transitions; } }
        }
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
        public SortedSet<NfaState> States { get; private set; }

        public bool HasFinalTransitions { get; private set; }
        public HashSet<NfaTransition> FinalTransitions { get; private set; }

        public bool AddState(NfaState state)
        {
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

        public void AddFinalTransition(NfaTransition nfaTransition)
        {
            if (FinalTransitions == null)
            {
                FinalTransitions = ReusableHashSet<NfaTransition>.GetAndClear();
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
