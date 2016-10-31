using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Automata.Nfa;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Parsing;
using RapidPliant.Parsing.Automata.Nfa;
using RapidPliant.Util;

namespace RapidPliant.Parsing.Automata.Dfa
{
    public class EarleyDfaBuilder : DfaBuilder
    {
        protected override DfaStateBuilder CreateStateBuilder(NfaClosure closure)
        {
            return new EarleyDfaStateBuilder(this, closure);
        }

        protected override DfaState CreateDfaState()
        {
            return new EarleyDfaState();
        }
    }

    public class EarleyDfaStateBuilder : DfaStateBuilder
    {
        public EarleyDfaStateBuilder(DfaBuilder dfaBuilder, NfaClosure closure)
            : base(dfaBuilder, closure)
        {
        }

        public void PostBuild()
        {
            //Now process the references 
        }
        
        protected override IDfaTransition CreateDfaTransition(object transitionValue, IEnumerable<INfaTransition> nfaTransitions, HashSet<INfaTransition> finalNfaTransitions, DfaState toDfaState)
        {
            var dfaTransition = new EarleyDfaTransition((int)transitionValue, nfaTransitions, finalNfaTransitions, toDfaState);
            
            //Collect completions for final nfa transitions
            CollectCompletions(dfaTransition, finalNfaTransitions);

            return dfaTransition;
        }

        protected override void BuildFornNfaTransition(INfaTransition transition)
        {
            //We only handle earley trnsitions
            var earleyTransition = transition as IEarleyNfaLexTransition;
            if(earleyTransition == null)
                return;

            var grammarDefId = earleyTransition.LexDef.Id;

            var ruleTransition = transition as IEarleyNfaResolveRuleTransition;
            if (ruleTransition != null)
            {
                //Rule transitions are treated the same way... except we want to add "predict information" to the "source state"
                //We also want to add "completion information" to the transition
            }

            var lexTransition = transition as IEarleyNfaLexTransition;
            if (lexTransition != null)
            {
                //Token transitions are treated the same way... except we want to add "scan information" to the transition
            }

            //Here we must take into account the different char intervals a terminal can have!
            if (earleyTransition.FromState == null)
            {
                throw new Exception("FromState is required!");
            }

            //Add the transition to the closure transitions
            ClosureTransitions.AddTransition(grammarDefId, earleyTransition);
        }

        protected override IEnumerable<INfaTransitionsByValue> GetNfaTransitionsByValue()
        {
            //Just return the transitions from the closure... those are already per grammar id
            return ClosureTransitions.GetTransitionsByTransitionValues();
        }
    }
}
