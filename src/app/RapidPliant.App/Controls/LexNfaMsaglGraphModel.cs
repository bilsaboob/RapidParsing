using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using RapidPliant.Lexing.Automata;

namespace RapidPliant.App.ViewModels
{
    public class LexNfaMsaglGraphModel : MsaglGraphModel<LexNfaAutomata.LexNfaState, LexNfaAutomata.LexNfaTransition>
    {
        public LexNfaMsaglGraphModel()
        {
        }

        protected override Graph CreateGraph()
        {
            return new LexNfaMsaglGraph();
        }

        protected override int GetStateId(LexNfaAutomata.LexNfaState state)
        {
            return state.Id;
        }

        protected override IEnumerable<LexNfaAutomata.LexNfaTransition> GetStateTransitions(LexNfaAutomata.LexNfaState state)
        {
            return state.Transitions;
        }
        
        protected override LexNfaAutomata.LexNfaState GetTransitionToState(LexNfaAutomata.LexNfaTransition transition)
        {
            return transition.ToState;
        }

        protected override string GetStateLabel(LexNfaAutomata.LexNfaState state)
        {
            return state.Id.ToString();
        }

        protected override string GetTransitionLabel(LexNfaAutomata.LexNfaTransition transition)
        {
            var terminalTransition = transition as ILexNfaTerminalTransition;
            if (terminalTransition != null)
            {
                return terminalTransition.Terminal.ToString();
            }

            var nullTransition = transition as ILexNfaNullTransition;
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