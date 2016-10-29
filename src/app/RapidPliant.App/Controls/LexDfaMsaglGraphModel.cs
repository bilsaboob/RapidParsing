using RapidPliant.Lexing.Automata;
using RapidPliant.Util;

namespace RapidPliant.App.ViewModels
{
    public class LexDfaMsaglGraphModel : DfaMsaglGraphModel
    {
        protected override string GetTransitionLabel(IDfaTransition transition)
        {
            var intervalLabel = "";
            var interval = transition.TransitionValue as Interval;
            if (interval != null)
            {
                if (interval.Min == interval.Max)
                {
                    intervalLabel = $"'{interval.Min}'";
                }
                else
                {
                    intervalLabel = interval.ToString();
                }
            }

            var terminalsLabel = "";
            var lexDfaTransition = transition as ILexDfaTransition;
            if (lexDfaTransition != null)
            {
                var terminals = lexDfaTransition.Terminals;
                
                foreach (var terminal in terminals)
                {
                    terminalsLabel += terminal.ToString() + ",";
                }
                terminalsLabel = terminalsLabel.Trim(',');
            }
            
            return $"({intervalLabel}) for: {terminalsLabel}";
        }
    }
}