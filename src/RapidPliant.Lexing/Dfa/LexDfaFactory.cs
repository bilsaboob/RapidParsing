using System.Collections.Generic;
using System.Linq;
using RapidPliant.Common.Rule;
using RapidPliant.Common.Util;
using RapidPliant.Lexing.Pattern;

namespace RapidPliant.Lexing.Dfa
{
    public class LexDfaFactory
    {
        public LexDfa BuildFromLexPatternRule(ILexPatternRule rule)
        {
            var dfa = new LexDfa();
            var startState = dfa.CreateState();
            dfa.StartState = startState;
            
            BuildForRule(rule, startState);

            //Make sure to compile the state once it has been built!
            startState.Compile();

            return dfa;
        }

        public LexDfa BuildFromLexPatternRules(IEnumerable<LexPatternRule> lexRules)
        {
            var dfa = new LexDfa();
            var startState = dfa.CreateState();
            dfa.StartState = startState;

            BuildForRules(lexRules, startState);

            //Make sure to compile the state once it has been built!
            startState.Compile();

            return dfa;
        }

        public void BuildForRule(IRule rule, LexDfaState startState)
        {
            var activePaths = rule.ToProductionPaths(startState);
            BuildForPaths(activePaths);
        }

        private void BuildForRules(IEnumerable<IRule> rules, LexDfaState startState)
        {
            var activePaths = rules.ToProductionPaths(startState);
            BuildForPaths(activePaths);
        }

        private void BuildForRulePaths(IRule rule, LexDfaState startState, RapidList<LexDfaProductionPath> nextPaths, LexDfaProductionPath parentPath = null)
        {
            var activePaths = rule.ToProductionPaths(startState, parentPath);
            BuildForPaths(activePaths, nextPaths);
        }

        public void BuildForPaths(RapidList<LexDfaProductionPath> activePaths)
        {
            var nextPaths = new RapidList<LexDfaProductionPath>(activePaths.Count);

            while (activePaths.Count > 0)
            {
                BuildForPaths(activePaths, nextPaths);

                //Build for the state of each active path!
                foreach (var activePath in activePaths)
                {
                    var state = activePath.State;
                    state.Build();

                    //Update the active state of the active path - the new state after the transition!
                    activePath.State = activePath.Transition.ToState;
                }

                //Get the next active paths to continue evaluating!
                activePaths = nextPaths.Clone();
                nextPaths.Clear();
            }
        }
        
        private void BuildForPaths(RapidList<LexDfaProductionPath> activePaths, RapidList<LexDfaProductionPath> nextPaths)
        {
            //Move one step ahead for each of the active paths!
            foreach (var activePath in activePaths)
            {
                var state = activePath.State;

                //Collect the relevant data from the path before we move ahead
                var symbol = activePath.Symbol;
                var pathProductionState = activePath.ToProductionState();
                
                if (!pathProductionState.IsAtEnd)
                {
                    //We are not at the end, so we either have a Prediction or a Scan - either way it's a transition!
                    state.AddSymbolTransition(symbol, pathProductionState);

                    //Now also check for prediction or scan
                    var symbolRuleRef = symbol as ILexPatternRuleRefSymbol;
                    if (symbolRuleRef != null)
                    {
                        //Reference to other rule - so we have a prediction
                        var refRule = symbolRuleRef.Rule;
                        state.AddPrediction(symbol, refRule, pathProductionState);

                        //Build for one step ahead for the expanded rules, next iteration they will be part of the "nextPaths"
                        BuildForRulePaths(refRule, state, nextPaths, activePath);
                    }
                    else
                    {
                        //We have a scan
                        state.AddScan(symbol, pathProductionState);
                    }

                    //Move one step ahead
                    activePath.MoveNext();

                    //Continue the activePath
                    nextPaths.Add(activePath);
                }
                else
                {
                    //We are at the end, so it's a completion
                    state.AddCompletion(symbol, pathProductionState);

                    var parentActivePath = activePath.ParentPath;
                    //Continue with the parent path if available
                    if (parentActivePath != null)
                    {
                        //Move One step ahead for the parent path
                        parentActivePath.MoveNext();
                        //There was a prediction from a parent path
                        nextPaths.Add(parentActivePath);
                    }

                    //The activePath will not be continued on a completion
                }
            }
        }
    }
}