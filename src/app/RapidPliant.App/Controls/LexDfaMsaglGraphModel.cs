using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using RapidPliant.Lexing.Graph;

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
            var terminal = transition.Terminal;
            if (terminal != null)
            {
                return terminal.ToString();
            }

            return base.GetTransitionLabel(transition);
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