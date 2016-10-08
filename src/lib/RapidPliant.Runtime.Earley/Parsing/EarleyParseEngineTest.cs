using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Runtime.Earley.Grammar;

namespace RapidPliant.Runtime.Earley.Parsing.Test
{
    

    public class SymbolRef
    {
        public Production Production { get; set; }
        public int Location { get; set; }
        public ISymbol Symbol { get; set; }
    }


    public class RuleDefinition
    {
    }

    public class ExprRuleDefinition : RuleDefinition
    {
    }

    public class Symbol
    {
    }

    public class RuleSymbol : Symbol
    {
        public Rule Rule { get; set; }
    }

    public class LexSymbol : Symbol
    {
    }
    
    

    public class DfaLexTransition
    {
        public List<ProductionState> ProductionStates { get; set; }

        public DfaState ToState { get; set; }

        public void AddProductionState(ProductionState productionState)
        {
            if(!ProductionStates.Contains(productionState))
                ProductionStates.Add(productionState);
        }
    }

    public class DfaState
    {
        public List<ProductionState> ProductionStates { get; set; }

        public List<DfaState> ToStates { get; set; }

        public DfaState(List<ProductionState> productionStates)
        {
            ProductionStates = productionStates;
        }

        private Dictionary<LexSymbol, DfaLexTransition> _lexTransitions;

        public void AddScanTransition(LexSymbol lexSymbol, ProductionState scanProductionState)
        {
            var lexTrans = _lexTransitions[lexSymbol];
            lexTrans.AddProductionState(scanProductionState);
        }

        public void AddPredictionTransition(RuleSymbol predictedRuleSymbol, ProductionState predictionProductionState)
        {
        }

        public void AddCompletion(ProductionState completedProductionState)
        {
        }

        public void CompileTransitions()
        {
            foreach (var lexTransition in _lexTransitions.Values)
            {
                var nextState = new DfaState(lexTransition.ProductionStates);
                lexTransition.ToState = nextState;
                ToStates.Add(nextState);
            }
        }
    }

    public class DfaBuilder
    {
        public DfaState BuildForRule(Rule rule)
        {
            var startState = new DfaState(rule.Productions.Select(p => new ProductionState(p, 0)).ToList());
            BuildForProductions(startState);
            return startState;
        }

        private DfaState BuildForSubRule(Rule rule)
        {
            return BuildForRule(rule);
        }

        private void BuildForProductions(DfaState state)
        {
            var productionStates = state.ProductionStates.ToList();

            while (productionStates.Count > 0)
            {
                var currentStates = productionStates.ToList();
                productionStates.Clear();
                foreach (var productionState in currentStates)
                {
                    if (productionState.IsEnd)
                    {
                        //Production path is at the end!
                        //We have a completion?
                        state.AddCompletion(productionState);
                        continue;
                    }

                    var symbol = productionState.Symbol;
                    var ruleSymbol = symbol as RuleSymbol;
                    if (ruleSymbol != null)
                    {
                        //We have a "prediction"
                        state.AddPredictionTransition(ruleSymbol, productionState);

                        if (ruleSymbol.Rule.IsSubRule)
                        {
                            //Evaluate sub rules right away... they can never point "back" to a parent... thus no risk for "recursion"
                            BuildForSubRule(ruleSymbol.Rule);
                        }

                        continue;
                    }

                    var lexSymbol = symbol as LexSymbol;
                    if (lexSymbol != null)
                    {
                        //We have a "scan"
                        state.AddScanTransition(lexSymbol, productionState);
                        continue;
                    }

                    productionStates.Add(productionState.Next());
                }
            }

            //Compile the transitions!
            state.CompileTransitions();

            //Build for the "to states"!
            foreach (var toState in state.ToStates)
            {
                BuildForProductions(toState);
            }
        }

        
    }

    public class ProductionState
    {
        public ProductionState(Production production, int symbolIndex)
        {
        }

        public Production Production { get; set; }

        public int SymbolIndex { get; set; }

        public bool IsEnd { get; set; }

        public ISymbol Symbol { get; set; }

        public ProductionState Next()
        {
            return new ProductionState(Production, SymbolIndex+1);
        }
    }

    public class ProductionSymbolIterator
    {
        public ProductionSymbolIterator(Production production)
        {
        }

        public ISymbol Symbol { get; set; }
        public Production Production { get; set; }
        public int SymbolIndex { get; set; }

        public bool MoveNext()
        {
            return false;
        }
    }

    public class Token
    {
        public int TokenType { get; set; }
    }

    public class NonTerminal
    {
    }

    public class ParseRunner
    {
        public ParseEngine Engine { get; set; }

        public void Pulse()
        {
            Dictionary<int, List<EngineAction>> nextScanActions = null;
            Engine.Init();

            var token = LexNext();

            var nextActions = nextScanActions[token.TokenType];
            foreach (var engineAction in nextActions)
            {
                engineAction.Scanned(token);
            }
        }

        private Token LexNext()
        {
            return null;
        }
    }

    public class EngineAction
    {
        public void Completed(NonTerminal nonTerminal)
        {
        }
        public virtual void Scanned(Token token)
        {
            //Specified token has been scanned!

        }
    }

    public class ScanAction : EngineAction
    {
        public ParseState State { get; set; }
        public TokenTransition Transition { get; set; }

        public ScanAction(ParseState state, TokenTransition transition)
        {
            State = state;
        }

        public override void Scanned(Token token)
        {
            var nextState = Transition.ToState;
            if (nextState.IsEnd)
            {
                //We have a completion!

            }
        }
    }
    
    public class ParseEngine
    {
        public EarleySet CurrentSet { get; set; }

        public void Init()
        {
            foreach (var startState in CurrentSet.States)
            {
                foreach (var tokenTransition in startState.TokenTransitions)
                {
                    NextExpectedScanActions.Add(tokenTransition.TokenType, new ScanAction(startState, tokenTransition));
                }
            }
        }

        public void Pulse()
        {
            foreach (var state in CurrentSet.States)
            {
            }
        }
    }

    public class EarleySet
    {
        public List<ParseState> States { get; set; }
    }

    public class ParseState
    {
    }
}
