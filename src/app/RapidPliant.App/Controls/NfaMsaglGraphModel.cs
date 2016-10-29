using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using RapidPliant.Lexing.Automata;

namespace RapidPliant.App.ViewModels
{
    public class NfaMsaglGraphModel : MsaglGraphModel<INfaState, INfaTransition>
    {
        public NfaMsaglGraphModel()
        {
        }

        protected override Graph CreateGraph()
        {
            return new NfaMsaglGraph();
        }

        protected override int GetStateId(INfaState state)
        {
            return state.Id;
        }

        protected override IEnumerable<INfaTransition> GetStateTransitions(INfaState state)
        {
            return state.Transitions;
        }
        
        protected override INfaState GetTransitionToState(INfaTransition transition)
        {
            return transition.ToState;
        }

        protected override string GetStateLabel(INfaState state)
        {
            return state.Id.ToString();
        }
    }

    public class NfaMsaglGraph : Graph
    {
    }
}