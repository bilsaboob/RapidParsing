using System;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Lexing.Automata;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Lexer
{
    public class LexDfaTableTransitionRecognizer : IDfaRecognizer<char>
    {
        public LexDfaTableTransitionRecognizer(DfaGraph dfaGraph)
        {
            Build(dfaGraph);
        }

        protected DfaTableRecognizerState StartState { get; set; }

        protected DfaTableRecognizerState State { get; set; }

        protected int[] CharToLookupTable { get; set; }

        protected virtual void Build(DfaGraph graph)
        {
            var states = new Dictionary<DfaState, DfaTableRecognizerState>();
            var intervalsByIntervalLookupIndex = new Dictionary<int, Interval>();
            //We need to split the terminal transitions of all DFA transitions into unique non overlapping intervals
            var nonOverlappingIntervalTransitions = new NonOverlappingIntervalSet<DfaTransition>();

            //Prepare recognizer states and simultaneously make sure to split transition intervals into non overlapping intervalls on DFA level... not on state level...
            foreach (var dfaFromState in graph.States)
            {
                states[dfaFromState] = new DfaTableRecognizerState(dfaFromState);

                //Process the dfa transitions, they are already non overlapping per state, but we need non overlapping per entire DFA
                foreach (var dfaTransition in dfaFromState.Transitions)
                {
                    dfaTransition.FromState = dfaFromState;

                    var interval = dfaTransition.Interval;
                    nonOverlappingIntervalTransitions.AddInterval(interval, dfaTransition);
                }
            }

            //We now have all non overlapping transitions in "nonOverlappingIntervalTransitions"

            var intervalLookupIndex = 1;

            //Process the dfa state transitions, now modifying for the non overlapping ones!
            foreach (var nonOverlappingIntervalTransition in nonOverlappingIntervalTransitions)
            {
                var interval = nonOverlappingIntervalTransition.Interval;
                var dfaTransitions = nonOverlappingIntervalTransition.AssociatedItems;
                
                //Process each of the initial dfa transitions, this time we will use the new intervals to add as transitions
                foreach (var dfaTransition in dfaTransitions)
                {
                    var dfaFromState = dfaTransition.FromState;
                    var dfaToState = dfaTransition.ToState;

                    var fromState = states[dfaFromState];
                    var toState = states[dfaToState];

                    var existing = fromState.Transitions.Where(t => t.Interval == interval).FirstOrDefault();
                    if (existing != null)
                    {
                        //Error...
                    }

                    fromState.AddTransition(new DfaTableRecognizerTransition(intervalLookupIndex, interval, dfaTransition, fromState, toState));
                }

                //Map the interval to the lookpu index
                intervalsByIntervalLookupIndex[intervalLookupIndex] = interval;

                //Increment the lookup index
                intervalLookupIndex++;
            }

            //Now finally build the char transitions table, intevalLookupIndex is the max index!
            foreach (var state in states.Values)
            {
                var transitionTable = new DfaTableRecognizerTransition[intervalLookupIndex];

                //Now populate the transition table for the transitions!
                foreach (var transition in state.Transitions)
                {
                    transitionTable[transition.IntervalLookupIndex] = transition;
                }

                state.TransitionsTable = transitionTable;
            }

            BuildCharToLookupTable(intervalsByIntervalLookupIndex);

            StartState = states[graph.StartState];
        }

        private void BuildCharToLookupTable(Dictionary<int, Interval> intervalsByIntervalLookupIndex)
        {
            //For now only use the max char value
            var maxCharValue = 256;
            var charToLookupTable = new int[maxCharValue];

            //Now map each character from the intervals into the lookup table - note that the interval must be non overlapping!
            foreach (var p in intervalsByIntervalLookupIndex)
            {
                var intervalLookupIndex = p.Key;
                var interval = p.Value;

                //Iterate the characters for the interval
                for (var ch = interval.Min; ch <= interval.Max; ++ch)
                {
                    charToLookupTable[ch] = intervalLookupIndex;
                }
            }

            CharToLookupTable = charToLookupTable;
        }

        public void Reset()
        {
            State = StartState;
        }

        public IDfaRecognition Recognize(char ch)
        {
            //Find the transition for the specified char
            var t = GetTransition(ch);
            if (t == null)
                return null;
            State = t.ToState;
            return t;
        }

        private DfaTableRecognizerTransition GetTransition(char ch)
        {
            //First get the lookup index for the specified char
            var intervalLookupIndex = CharToLookupTable[ch];
            if (intervalLookupIndex <= 0)
                return null;
            
            //Now access the transition from the transitions table
            return State.TransitionsTable[intervalLookupIndex];
        }
    }

    public class DfaTableRecognizerState
    {
        private List<DfaTableRecognizerTransition> _transitions;

        public DfaTableRecognizerState(DfaState dfaState)
        {
            DfaState = dfaState;

            _transitions = new List<DfaTableRecognizerTransition>();
        }
        
        public DfaState DfaState { get; private set; }

        public IReadOnlyList<DfaTableRecognizerTransition> Transitions { get { return _transitions; } }

        public DfaTableRecognizerTransition[] TransitionsTable { get; set; }

        public void AddTransition(DfaTableRecognizerTransition transition)
        {
            _transitions.Add(transition);
        }
    }

    public class DfaTableRecognizerTransition : IDfaRecognition
    {
        private List<IRecognizerCompletion> _completions;

        public DfaTableRecognizerTransition(int intervalLookupIndex, Interval interval, DfaTransition dfaTransition, DfaTableRecognizerState fromState, DfaTableRecognizerState toState)
        {
            IntervalLookupIndex = intervalLookupIndex;

            Interval = interval;

            DfaTransition = dfaTransition;

            FromState = fromState;
            ToState = toState;

            _completions = new List<IRecognizerCompletion>();

            BuildCompletions();
        }

        private void BuildCompletions()
        {
            var completions = DfaTransition.CompletionsByExpression;
            if(completions == null) 
                return;

            foreach (var dfaCompletion in completions)
            {
                _completions.Add(new DfaRecognizerCompletion(dfaCompletion));
            }
        }

        public int IntervalLookupIndex { get; private set; }

        public Interval Interval { get; private set; }

        public DfaTransition DfaTransition { get; private set; }

        public DfaTableRecognizerState FromState { get; private set; }
        
        public DfaTableRecognizerState ToState { get; private set; }
        
        DfaState IDfaRecognition.ToState { get { return ToState.DfaState; } }

        DfaState IDfaRecognition.FromState { get { return FromState.DfaState; } }

        public IEnumerable<IRecognizerCompletion> Completions { get { return _completions; } }
    }
}