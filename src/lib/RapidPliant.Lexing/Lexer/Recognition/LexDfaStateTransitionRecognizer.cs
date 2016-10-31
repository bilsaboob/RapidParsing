using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Lexer.Recognition;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Lexer
{
    public class LexDfaStateTransitionRecognizer : ILexRecognizer
    {
        public LexDfaStateTransitionRecognizer(DfaGraph dfaGraph)
        {
            Build(dfaGraph);
        }

        protected DfaRecognizerState StartState { get; set; }

        protected DfaRecognizerState State { get; set; }

        protected virtual void Build(DfaGraph graph)
        {
            var states = new Dictionary<IDfaState, DfaRecognizerState>();

            //Prepare the recognizer states, wrappers around the dfa states
            foreach (var dfaState in graph.States)
            {
                states[dfaState] = new DfaRecognizerState(dfaState);
            }

            foreach (var fromState in states.Values)
            {
                var dfaState = fromState.DfaState;

                foreach (var dfaTransition in dfaState.Transitions)
                {
                    var toDfaState = dfaTransition.ToState;
                    var toState = states[toDfaState];

                    fromState.AddTransition(new DfaRecognizerTransition(dfaTransition, fromState, toState));
                }
            }

            StartState = states[graph.StartState];
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

        private DfaRecognizerTransition GetTransition(char ch)
        {
            var transitions = State.Transitions;
            var transitionsCount = transitions.Count;

            for (var i = 0; i < transitionsCount; ++i)
            {
                var transition = transitions[i];

                var interval = transition.DfaTransition.TransitionValue as Interval;
                if (interval == null)
                    continue;

                if (ch < interval.Min)
                    continue;

                if (ch > interval.Max)
                    continue;

                return transition;
            }

            return null;
        }
    }

    public class DfaRecognizerState
    {
        private List<DfaRecognizerTransition> _transitions;

        public DfaRecognizerState(IDfaState dfaState)
        {
            DfaState = dfaState;

            _transitions = new List<DfaRecognizerTransition>();
        }

        public IDfaState DfaState { get; private set; }

        public IReadOnlyList<DfaRecognizerTransition> Transitions { get { return _transitions; } }

        public void AddTransition(DfaRecognizerTransition transition)
        {
            _transitions.Add(transition);
        }
    }

    public class DfaRecognizerTransition : IDfaRecognition
    {
        private List<IRecognizerCompletion> _completions;

        public DfaRecognizerTransition(IDfaTransition dfaTransition, DfaRecognizerState fromState, DfaRecognizerState toState)
        {
            DfaTransition = dfaTransition;

            FromState = fromState;
            ToState = toState;

            _completions = new List<IRecognizerCompletion>();

            BuildCompletions();
        }

        private void BuildCompletions()
        {
            var completions = DfaTransition.CompletionsByExpression;
            if (completions == null)
                return;

            foreach (var dfaCompletion in completions)
            {
                _completions.Add(new DfaRecognizerCompletion(dfaCompletion));
            }
        }

        public IDfaTransition DfaTransition { get; private set; }

        public DfaRecognizerState FromState { get; private set; }

        public DfaRecognizerState ToState { get; private set; }

        IDfaState IDfaRecognition.ToState { get { return ToState.DfaState; } }

        IDfaState IDfaRecognition.FromState { get { return FromState.DfaState; } }

        public IReadOnlyList<IRecognizerCompletion> Completions { get { return _completions; } }
    }

    public class DfaRecognizerCompletion : IRecognizerCompletion
    {
        public DfaRecognizerCompletion(DfaCompletion dfaCompletion)
        {
            DfaCompletion = dfaCompletion;
        }

        public DfaCompletion DfaCompletion { get; private set; }

        public object CompletionInfo { get { return DfaCompletion; } }

        public object CompletedValue { get { return DfaCompletion.Expression; } }
    }
}
