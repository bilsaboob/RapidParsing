using RapidPliant.Automata.Nfa;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Automata.Nfa;
using RapidPliant.Parsing.Automata.Nfa;

namespace RapidPliant.App.ViewModels
{
    public class EarleyNfaMsaglGraphModel : NfaMsaglGraphModel
    {
        protected override string GetTransitionLabel(INfaTransition transition)
        {
            var lexTransition = transition as IEarleyNfaLexTransition;
            if (lexTransition != null)
            {
                var lexDef = lexTransition.LexDef;
                return $"{lexDef.Id}:{lexDef.Name}:{lexDef.LexModel}";
            }

            var rulePredictionTransition = transition as IEarleyNfaRulePredictionTransition;
            if (rulePredictionTransition != null)
            {
                var ruleDef = rulePredictionTransition.RuleDef;
                return $"P:{ruleDef.Id}:{ruleDef.Name}";
            }

            var ruleCompletionTransition = transition as IEarleyNfaRuleCompletionTransition;
            if (ruleCompletionTransition != null)
            {
                var ruleDef = ruleCompletionTransition.RuleDef;
                return $"C:{ruleDef.Id}:{ruleDef.Name}";
            }

            var ruleTransition = transition as IEarleyNfaRuleTransition;
            if (ruleTransition != null)
            {
                var ruleDef = ruleTransition.RuleDef;
                return $"R:{ruleDef.Id}:{ruleDef.Name}";
            }

            var nullTransition = transition as IEarleyNfaNullTransition;
            if (nullTransition != null)
            {
                //Null transitions have no label yet... until Prediction and such...
                return "";
            }

            return base.GetTransitionLabel(transition);
        }
    }
}