using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using RapidPliant.Lexing.Graph;

namespace RapidPliant.App.ViewModels
{
    public class LexNfaMsaglGraphModel : MsaglGraphModel<NfaState, NfaTransition>
    {
        public LexNfaMsaglGraphModel()
        {
        }

        protected override Graph CreateGraph()
        {
            return new LexNfaMsaglGraph();
        }

        protected override int GetStateId(NfaState state)
        {
            return state.Id;
        }

        protected override IEnumerable<NfaTransition> GetStateTransitions(NfaState state)
        {
            return state.Transitions;
        }
        
        protected override NfaState GetTransitionToState(NfaTransition transition)
        {
            return transition.ToState;
        }

        protected override string GetStateLabel(NfaState state)
        {
            return state.Id.ToString();
        }

        protected override string GetTransitionLabel(NfaTransition transition)
        {
            var terminalTransition = transition as TerminalNfaTransition;
            if (terminalTransition != null)
            {
                return terminalTransition.Terminal.ToString();
            }

            var nullTransition = transition as NullNfaTransition;
            if (nullTransition != null)
            {
                //Null transitions have no label yet... until Prediction and such...
                return "";
            }

            return base.GetTransitionLabel(transition);
        }
    }

    public class LexNfaMsaglGraph : Graph
    {
        public override string ToString()
        {
            return base.ToString();
        }
    }
}