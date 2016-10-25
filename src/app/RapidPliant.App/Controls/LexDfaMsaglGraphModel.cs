using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;
using RapidPliant.Lexing.Automata;

namespace RapidPliant.App.ViewModels
{
    public class LexDfaMsaglGraphModel : MsaglGraphModel<DfaState, DfaTransition>
    {
        public LexDfaMsaglGraphModel()
        {
        }

        protected override Graph CreateGraph()
        {
            return new LexDfaMsaglGraph();
        }

        protected override int GetStateId(DfaState state)
        {
            return state.Id;
        }

        protected override IEnumerable<DfaTransition> GetStateTransitions(DfaState state)
        {
            return state.Transitions;
        }

        protected override DfaState GetTransitionToState(DfaTransition transition)
        {
            return transition.ToState;
        }

        protected override string GetStateLabel(DfaState state)
        {
            return state.Id.ToString();
        }

        protected override string GetTransitionLabel(DfaTransition transition)
        {
            var intervalLabel = "";
            var interval = transition.Interval;
            if (interval.Min == interval.Max)
            {
                intervalLabel = $"'{interval.Min}'";
            }
            else
            {
                intervalLabel = interval.ToString();
            }

            var terminals = transition.Terminals;
            var terminalsLabel = "";
            foreach (var terminal in terminals)
            {
                terminalsLabel += terminal.ToString() + ",";
            }
            terminalsLabel = terminalsLabel.Trim(',');

            return $"({intervalLabel}) for: {terminalsLabel}";

            //return base.GetTransitionLabel(transition);
        }

        protected override bool IsFinalState(DfaState state)
        {
            var isFinal = base.IsFinalState(state);
            if (isFinal)
                return true;

            return state.IsFinal;
        }
    }

    public class LexDfaMsaglGraph : Graph
    {
        public override string ToString()
        {
            return base.ToString();
        }
    }
}