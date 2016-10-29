using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;
using RapidPliant.Automata.Dfa;
using RapidPliant.Lexing.Automata;

namespace RapidPliant.App.ViewModels
{
    public class DfaMsaglGraphModel : MsaglGraphModel<IDfaState, IDfaTransition>
    {
        public DfaMsaglGraphModel()
        {
        }

        protected override Graph CreateGraph()
        {
            return new DfaMsaglGraph();
        }

        protected override int GetStateId(IDfaState state)
        {
            return state.Id;
        }

        protected override IEnumerable<IDfaTransition> GetStateTransitions(IDfaState state)
        {
            return state.Transitions;
        }

        protected override IDfaState GetTransitionToState(IDfaTransition transition)
        {
            return transition.ToState;
        }

        protected override string GetStateLabel(IDfaState state)
        {
            return state.Id.ToString();
        }

        protected override bool IsFinalState(IDfaState state)
        {
            var isFinal = base.IsFinalState(state);
            if (isFinal)
                return true;

            return state.IsFinal;
        }
    }

    public class DfaMsaglGraph : Graph
    {
        public override string ToString()
        {
            return base.ToString();
        }
    }
}