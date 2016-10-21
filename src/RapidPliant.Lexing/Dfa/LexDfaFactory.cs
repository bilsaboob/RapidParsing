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

            return dfa;
        }

        public LexDfa BuildFromLexPatternRules(IEnumerable<LexPatternRule> lexRules)
        {
            var dfa = new LexDfa();
            var startState = dfa.CreateState();
            dfa.StartState = startState;

            BuildForRules(lexRules, startState);

            return dfa;
        }

        public void BuildForRule(IRule rule, LexDfaState startState)
        {
            var activePaths = new RapidList<LexDfaProductionPath>(rule.Productions.Select(p => new LexDfaProductionPath(p, startState)));
            BuildForPaths(activePaths);
        }

        private void BuildForRules(IEnumerable<IRule> rules, LexDfaState startState)
        {
            var activePaths = new RapidList<LexDfaProductionPath>(rules.SelectMany(rule => rule.Productions.Select(p => new LexDfaProductionPath(p, startState))));
            BuildForPaths(activePaths);
        }

        private void BuildForRulePaths(IRule rule, LexDfaState startState, RapidList<LexDfaProductionPath> nextPaths, LexDfaProductionPath parentPath = null)
        {
            var activePaths = new RapidList<LexDfaProductionPath>(rule.Productions.Select(p => new LexDfaProductionPath(p, startState, parentPath)));

            BuildForPaths(activePaths, nextPaths);
        }

        public void BuildForPaths(RapidList<LexDfaProductionPath> activePaths)
        {
            var nextPaths = new RapidList<LexDfaProductionPath>(activePaths.Count);

            while (activePaths.Count > 0)
            {
                BuildForPaths(activePaths, nextPaths);
                
                //Now make sure to compile the states, building the "next" state!
                foreach (var activePath in activePaths)
                {
                    //Compile the state - this will build the next state, given the transitions that have been added!
                    var state = activePath.State;
                    state.Compile();

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

                //Get the symbol for which to make a transition!
                var symbol = activePath.Symbol;

                if (activePath.IsAtEnd)
                {
                    //The path is at the end - this means we have a completion!
                    var parentActivePath = activePath.ParentPath;
                    if (parentActivePath != null)
                    {
                        //There was a prediction from a parent path
                        var parentState = parentActivePath.State;
                        var completedSymbol = parentActivePath.Symbol;
                        var transition = state.AddSymbolTransition(symbol, activePath.Clone());
                        activePath.Transition = transition;
                        state.AddCompletion(completedSymbol);

                        //Move the parent path one step ahead and add to the active paths to be evaluated!
                        parentActivePath.MoveNext();
                        nextPaths.Add(parentActivePath);
                    }
                    else
                    {
                        //There is no parent path - so let's add the symbol as a normal transition!
                        /*var transition = state.AddSymbolTransition(symbol, activePath.Clone());
                        activePath.Transition = transition;*/
                        state.AddCompletion(symbol);
                    }
                }
                else
                {
                    var symbolRuleRef = symbol as ILexPatternRuleRefSymbol;
                    if (symbolRuleRef != null)
                    {
                        //This is a reference to another sub lex rule - this means it's a prediction!
                        var refRule = symbolRuleRef.Rule;
                        //Build for one step ahead for the expanded rules... next iteration they will be part of the "nextPaths"
                        BuildForRulePaths(refRule, state, nextPaths, activePath);
                        state.AddPrediction(symbolRuleRef);

                        //Don't add the current path to next iteration list... we must have a completion to continue!
                    }
                    else
                    {
                        //Add as a normal transition!
                        var transition = state.AddSymbolTransition(symbol, activePath.Clone());
                        activePath.Transition = transition;
                        state.AddScan(symbol);

                        //Move the path one step ahead, and continue the next iteration
                        activePath.MoveNext();
                        nextPaths.Add(activePath);
                    }
                }
            }
        }
    }
}