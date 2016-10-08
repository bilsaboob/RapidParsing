using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Common.Util;
using RapidPliant.Lexing.Pattern;

namespace RapidPliant.Lexing.Dfa
{
    public class LexDfaState
    {
        private RapidTable<ISymbol, LexDfaStateTransition> Transitions { get; set; }
        private RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition> TermTransitions { get; set; }
        
        public LexDfaState()
        {
            Transitions = new RapidTable<ISymbol, LexDfaStateTransition>();
            TermTransitions = new RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition>();
        }
        
        public void AddSymbolTransition(ISymbol symbol, LexDfaProductionState productionState)
        {
            var termSymbol = symbol as ILexPatternTerminalSymbol;
            if (termSymbol != null)
            {
                AddTerminalSymbolTransition(termSymbol, productionState);
                return;
            }

            var ruleRefSymbol = symbol as ILexPatternRuleRefSymbol;
            if (ruleRefSymbol != null)
            {
                AddRuleSymbolTransition(ruleRefSymbol, productionState);
                return;
            }
        }
        
        private void AddTerminalSymbolTransition(ILexPatternTerminalSymbol terminalSymbol, LexDfaProductionState productionState)
        {
            //Either get the existing or add a new one
            var termTransition = TermTransitions.AddOrGetExisting(terminalSymbol, ()=>new LexDfaStateTermTransition(terminalSymbol));
            
            //Add the production state to the transition
            termTransition.AddProductionState(productionState);
        }

        private void AddRuleSymbolTransition(ILexPatternRuleRefSymbol ruleSymbol, LexDfaProductionState productionState)
        {
        }

        public LexDfaStateTermTransition[] GetTermTransitions()
        {
            //Get the term transition values!
            return TermTransitions.Values;
        }

        public void Compile()
        {
            //Compile each of the transitions!
            foreach (var transition in GetTermTransitions())
            {
                transition.Compile();
            }
        }
    }
    
    public class LexDfaStateCompletion
    {
        private CachingRapidList<LexDfaProductionState> _productionStates;

        public LexDfaStateCompletion(IRule completedLexRule)
        {
            Rule = completedLexRule;
            _productionStates = new CachingRapidList<LexDfaProductionState>();
        }

        public IRule Rule { get; private set; }

        public int ProductionStatesCount { get { return _productionStates.Count; } }
        public LexDfaProductionState[] ProductionStates { get { return _productionStates.AsArray; } }

        public void AddProductionState(LexDfaProductionState productionState)
        {
            _productionStates.Add(productionState);
        }
    }

    public class LexDfaStateTransition
    {
        private CachingRapidList<LexDfaProductionState> _productionStates;
        private CachingRapidList<LexDfaProductionState> _completionStates;

        public LexDfaStateTransition(ILexPatternSymbol transSymbol)
        {
            TransitionSymbol = transSymbol;
            _productionStates = new CachingRapidList<LexDfaProductionState>();
        }
        
        public ILexPatternSymbol TransitionSymbol { get; private set; }

        public int ProductionStatesCount { get { return _productionStates.Count; } }
        public LexDfaProductionState[] ProductionStates { get { return _productionStates.AsArray; } }

        public int CompletionsCount { get; private set; }
        public LexDfaStateCompletion[] CompletionsByRule { get; private set; }

        public void AddProductionState(LexDfaProductionState productionState)
        {
            _productionStates.Add(productionState);

            if (productionState.IsEnd)
            {
                //Add as a completion too!
                _completionStates.Add(productionState);
            }
        }

        public void Compile()
        {
            //BuIld the 
            var completionsByRule = new RapidTable<IRule, LexDfaStateCompletion>();

            foreach (var completionProductionState in _completionStates.AsArray)
            {
                var completedRule = completionProductionState.Production.LhsRule;
                var completion = completionsByRule.AddOrGetExisting(completedRule, () => new LexDfaStateCompletion(completedRule));
                completion.AddProductionState(completionProductionState);
            }
        }
    }

    public class LexDfaStateTermTransition : LexDfaStateTransition
    {
        public LexDfaStateTermTransition(ILexPatternTerminalSymbol termSymbol)
            : base(termSymbol)
        {
            TransitionTermSymbol = termSymbol;
        }

        public ILexPatternTerminalSymbol TransitionTermSymbol { get; private set; }
        
    }

    public class LexDfaFactory
    {
        public LexDfaState BuildFromLexPatternRule(ILexPatternRule rule)
        {
            var startState = new LexDfaState();

            var activePaths = rule.Productions.Select(p => new LexDfaProductionPath(p, startState)).ToList();
            var nextPaths = new RapidList<LexDfaProductionPath>(activePaths.Count);

            while (activePaths.Count > 0)
            {
                //Move one step ahead for each of the active paths!
                foreach (var activePath in activePaths)
                {
                    var fromState = activePath.FromState;

                    //Move the path one step ahead
                    if(!activePath.MoveNext())
                        continue;

                    if (activePath.IsAtEnd)
                    {
                        //The path is finished
                    }
                    else
                    {
                        //Get the next transition symbol!
                        var symbol = activePath.Symbol;
                        fromState.AddSymbolTransition(symbol, activePath.ToProductionState());
                    }

                    nextPaths.Add(activePath);
                }

                //Get the next active paths to continue evaluating!
                activePaths = nextPaths.ToList();
            }

            if (rule.HasSubRules)
            {
                //Iterate the sub rules and 
                foreach (var subRule in rule.SubRules)
                {
                }
            }

            return startState;
        }
    }

    public class LexDfaProductionState
    {
        public LexDfaProductionState(LexDfaProductionPath forPath)
        {
            Production = forPath.Production;
            SymbolIndex = forPath.SymbolIndex;
            Symbol = forPath.Symbol;
            IsEnd = forPath.IsAtEnd;
        }

        public IProduction Production { get; private set; }
        public int SymbolIndex { get; private set; }
        public ISymbol Symbol { get; private set; }
        public bool IsEnd { get; private set; }
    }

    public class LexDfaProductionPath
    {
        private IProduction _production;
        private int _rhsSymbolsCount;
        private ISymbol[] _rhsSymbols;
        private int _rhsIndex;
        private bool _isAtEnd;
        private int _rhsEndIndex;
        private ISymbol _symbol;
        private LexDfaState _fromState;
        private LexDfaProductionState _productionState;

        public LexDfaProductionPath(IProduction production, LexDfaState fromState)
        {
            _fromState = fromState;

            _production = production;
            _rhsIndex = 0;

            _rhsSymbolsCount = Production.RhsSymbolsCount;
            _rhsSymbols = Production.RhsSymbols;
            _rhsEndIndex = _rhsSymbolsCount - 1;

            _isAtEnd = _rhsIndex >= _rhsSymbolsCount;
        }

        public LexDfaState FromState { get { return _fromState; } }

        public int SymbolIndex { get { return _rhsIndex; } }
        public IProduction Production { get { return _production; } }

        public bool IsAtEnd { get { return _isAtEnd; } }
        public ISymbol Symbol { get { return _symbol; } }

        public bool MoveNext()
        {
            if (_isAtEnd)
                return false;

            if (_rhsIndex >= _rhsEndIndex)
            {
                _isAtEnd = true;
                return false;
            }

            _rhsIndex++;
            _symbol = _rhsSymbols[_rhsIndex];
            _productionState = null;
            return true;
        }

        public LexDfaProductionState ToProductionState()
        {
            if(_productionState == null)
            {
                _productionState = new LexDfaProductionState(this);
            }

            return _productionState;
        }
    }
}
