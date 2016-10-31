using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;

namespace RapidPliant.Automata.Dfa
{
    public interface IDfaRecognizer
    {
    }

    public interface IDfaRecognizer<TInput> : IDfaRecognizer
    {
        IDfaRecognition Recognize(TInput input);

        void Reset();
    }

    public interface IDfaRecognition
    {
        IDfaState FromState { get; }
        IDfaState ToState { get; }
        IReadOnlyList<IRecognizerCompletion> Completions { get; }
    }

    public interface IRecognizerCompletion
    {
        object CompletionInfo { get; }
        object CompletedValue { get; }
    }

    public abstract class DfaRecognizer<TTransitionValue, TState, TTransition>
        where TState : DfaRecognizerState<TTransitionValue, TState, TTransition>
        where TTransition : DfaRecognizerTransition<TTransitionValue, TState, TTransition>
    {
        public DfaRecognizer()
        {
        }

        public TState StartState { get; protected set; }
        public TState State { get; protected set; }

        public void Reset()
        {
            Reset(StartState);
        }

        public void Reset(TState atState)
        {
            State = atState;
        }

        protected virtual void Build(IDfaGraph graph)
        {
            var states = new Dictionary<IDfaState, TState>();

            //Prepare the recognizer states, wrappers around the dfa states
            foreach (var dfaState in graph.States)
            {
                states[dfaState] = CreateRecognizerState(dfaState);
            }

            foreach (var fromState in states.Values)
            {
                var dfaState = fromState.DfaState;

                foreach (var dfaTransition in dfaState.Transitions)
                {
                    var toDfaState = dfaTransition.ToState;
                    var toState = states[toDfaState];

                    fromState.AddTransition(CreateRecognizerTransition(dfaTransition, fromState, toState));
                }
            }

            StartState = states[graph.StartState];
        }
        
        public virtual IDfaRecognition Recognize(TTransitionValue value)
        {
            //Find the transition for the specified char
            var t = GetTransition(value);
            if (t == null)
                return null;
            State = t.ToState;
            return t;
        }

        protected abstract TState CreateRecognizerState(IDfaState dfaState);
        protected abstract TTransition CreateRecognizerTransition(IDfaTransition dfaTransition, TState fromState, TState toState);
        
        protected abstract TTransition GetTransition(TTransitionValue value);
    }


    public class DfaRecognizerState<TTransitionValue, TState, TTransition>
        where TState : DfaRecognizerState<TTransitionValue, TState, TTransition>
        where TTransition : DfaRecognizerTransition<TTransitionValue, TState, TTransition>
    {
        private List<TTransition> _transitions;

        public DfaRecognizerState(IDfaState dfaState)
        {
            DfaState = dfaState;

            _transitions = new List<TTransition>();
        }

        public IDfaState DfaState { get; private set; }

        public IReadOnlyList<TTransition> Transitions { get { return _transitions; } }

        public void AddTransition(TTransition transition)
        {
            _transitions.Add(transition);
        }
    }

    public class DfaRecognizerTransition<TTransitionValue, TState, TTransition> : IDfaRecognition
        where TState : DfaRecognizerState<TTransitionValue, TState, TTransition>
        where TTransition : DfaRecognizerTransition<TTransitionValue, TState, TTransition>
    {
        private List<IRecognizerCompletion> _completions;

        public DfaRecognizerTransition(IDfaTransition dfaTransition, TState fromState, TState toState)
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

        public TState FromState { get; private set; }

        public TState ToState { get; private set; }

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
