using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Util;

namespace RapidPliant.Parsing.Earley.Recognition
{
    public class EarleyDfaStateTransitionRecognizer : DfaRecognizer<int, EarleyDfaStateTransitionRecognizerState, EarleyDfaStateTransitionRecognizerTransition>, IEarleyRecognizer
    {
        public EarleyDfaStateTransitionRecognizer(IDfaGraph dfaGraph)
        {
            Build(dfaGraph);
        }

        protected override EarleyDfaStateTransitionRecognizerState CreateRecognizerState(IDfaState dfaState)
        {
            return new EarleyDfaStateTransitionRecognizerState(dfaState);
        }

        protected override EarleyDfaStateTransitionRecognizerTransition CreateRecognizerTransition(IDfaTransition dfaTransition, EarleyDfaStateTransitionRecognizerState fromState, EarleyDfaStateTransitionRecognizerState toState)
        {
            return new EarleyDfaStateTransitionRecognizerTransition(dfaTransition, fromState, toState);
        }
        
        protected override EarleyDfaStateTransitionRecognizerTransition GetTransition(int grammarDefId)
        {
            var transitions = State.Transitions;
            var transitionsCount = transitions.Count;

            for (var i = 0; i < transitionsCount; ++i)
            {
                var transition = transitions[i];
                var transitionValue = (int)transition.DfaTransition.TransitionValue;

                if (transitionValue == grammarDefId)
                    return transition;
            }

            return null;
        }
    }

    public class EarleyDfaStateTransitionRecognizerState : DfaRecognizerState<int, EarleyDfaStateTransitionRecognizerState, EarleyDfaStateTransitionRecognizerTransition>
    {
        public EarleyDfaStateTransitionRecognizerState(IDfaState dfaState)
            : base(dfaState)
        {
        }
    }

    public class EarleyDfaStateTransitionRecognizerTransition : DfaRecognizerTransition<int, EarleyDfaStateTransitionRecognizerState, EarleyDfaStateTransitionRecognizerTransition>
    {
        public EarleyDfaStateTransitionRecognizerTransition(IDfaTransition dfaTransition, EarleyDfaStateTransitionRecognizerState fromState, EarleyDfaStateTransitionRecognizerState toState)
            : base(dfaTransition, fromState, toState)
        {
        }
    }
}
