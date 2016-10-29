using RapidPliant.Automata.Nfa;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Automata.Nfa;

namespace RapidPliant.App.ViewModels
{
    public class LexNfaMsaglGraphModel : NfaMsaglGraphModel
    {
        protected override string GetTransitionLabel(INfaTransition transition)
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
}