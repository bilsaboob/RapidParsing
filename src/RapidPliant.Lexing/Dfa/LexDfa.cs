using System;
using System.CodeDom;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Common.Util;
using RapidPliant.Lexing.Pattern;

namespace RapidPliant.Lexing.Dfa
{
    public class LexDfa
    {
        private CachingRapidList<LexDfaState> _states;
        private int _nextStateId;

        public LexDfa()
        {
            _nextStateId = 1;
            _states = new CachingRapidList<LexDfaState>();
        }

        public LexDfaState StartState { get; set; }

        public int StatesCount { get { return _states.Count; } }

        public LexDfaState[] States { get { return _states.AsArray; } }

        public LexDfaState CreateState()
        {
            var state = new LexDfaState(this);
            state.StateId = _nextStateId++;
            _states.Add(state);
            return state;
        }
    }

    public class LexDfaState
    {
        private LexDfa _dfa;
        private bool _hasCompiled;

        private RapidTable<ISymbol, LexDfaStateTransition> Transitions { get; set; }
        private RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition> TermTransitions { get; set; }
        
        public int StateId { get; set; }

        public LexDfaState(LexDfa dfa)
        {
            _dfa = dfa;

            Transitions = new RapidTable<ISymbol, LexDfaStateTransition>();
            TermTransitions = new RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition>();
        }

        public LexDfaStateTransition AddSymbolTransition(ISymbol symbol, LexDfaProductionPath productionPath)
        {
            var termSymbol = symbol as ILexPatternTerminalSymbol;
            if (termSymbol != null)
            {
                return AddTerminalSymbolTransition(termSymbol, productionPath);
            }

            var ruleRefSymbol = symbol as ILexPatternRuleRefSymbol;
            if (ruleRefSymbol != null)
            {
                return AddRuleSymbolTransition(ruleRefSymbol, productionPath);
            }

            return null;
        }

        private LexDfaStateTransition AddTerminalSymbolTransition(ILexPatternTerminalSymbol terminalSymbol, LexDfaProductionPath productionState)
        {
            //Either get the existing or add a new one
            var termTransition = TermTransitions.AddOrGetExisting(terminalSymbol, () => new LexDfaStateTermTransition(terminalSymbol));

            //Add the production state to the transition
            termTransition.AddProductionPath(productionState);

            _hasCompiled = false;

            return termTransition;
        }

        private LexDfaStateTransition AddRuleSymbolTransition(ILexPatternRuleRefSymbol ruleSymbol, LexDfaProductionPath productionState)
        {
            _hasCompiled = false;
            return null;
        }

        public LexDfaStateTermTransition[] GetTermTransitions()
        {
            //Get the term transition values!
            return TermTransitions.Values;
        }

        public void Compile()
        {
            if(_hasCompiled)
                return;

            //Compile each of the transitions!
            foreach (var transition in GetTermTransitions())
            {
                //Create a next state for each transition
                transition.Compile();

                var nextState = _dfa.CreateState();
                transition.SetToState(nextState);
            }

            _hasCompiled = true;
        }

        public void AddPrediction(ILexPatternRuleRefSymbol symbolRuleRef)
        {
            //Add a prediction info?
        }

        public void AddCompletion(ISymbol completedSymbol)
        {
            //Add a completion info?
        }

        public void AddScan(ISymbol symbol)
        {
            //Add scan info?
        }

        public override string ToString()
        {
            return StateId.ToString();
        }
    }

    public class LexDfaStateCompletion
    {
        private CachingRapidList<LexDfaProductionPath> _productionPaths;

        public LexDfaStateCompletion(IRule completedLexRule)
        {
            Rule = completedLexRule;
            _productionPaths = new CachingRapidList<LexDfaProductionPath>();
        }

        public IRule Rule { get; private set; }

        public int ProductionStatesCount { get { return _productionPaths.Count; } }
        public LexDfaProductionPath[] ProductionStates { get { return _productionPaths.AsArray; } }

        public void AddProductionPath(LexDfaProductionPath productionState)
        {
            _productionPaths.Add(productionState);
        }
    }

    public class LexDfaStateTransition
    {
        private CachingRapidList<LexDfaProductionPath> _productionPaths;
        private CachingRapidList<LexDfaProductionPath> _completionPaths;
        private bool _requiresCompile;

        public LexDfaStateTransition(ILexPatternSymbol transSymbol)
        {
            TransitionSymbol = transSymbol;
            _productionPaths = new CachingRapidList<LexDfaProductionPath>();
            _completionPaths = new CachingRapidList<LexDfaProductionPath>();
            _requiresCompile = true;
        }

        public ILexPatternSymbol TransitionSymbol { get; private set; }

        public int ProductionStatesCount { get { return _productionPaths.Count; } }
        public LexDfaProductionPath[] ProductionPaths { get { return _productionPaths.AsArray; } }

        public int CompletionsCount { get; private set; }
        public LexDfaStateCompletion[] CompletionsByRule { get; private set; }

        public LexDfaState ToState { get; private set; }

        public void AddProductionPath(LexDfaProductionPath productionState)
        {
            _productionPaths.Add(productionState);

            if (productionState.IsAtEnd)
            {
                //Add as a completion too!
                _completionPaths.Add(productionState);
            }

            _requiresCompile = true;
        }

        public void Compile()
        {
            if(!_requiresCompile)
                return;

            //Build the
            var completionsByRule = new RapidTable<IRule, LexDfaStateCompletion>();

            if (_completionPaths.Count > 0)
            {
                foreach (var completionProductionState in _completionPaths.AsArray)
                {
                    var completedRule = completionProductionState.Production.LhsRule;
                    var completion = completionsByRule.AddOrGetExisting(completedRule, () => new LexDfaStateCompletion(completedRule));
                    completion.AddProductionPath(completionProductionState);
                }
            }

            CompletionsByRule = completionsByRule.Values;
            CompletionsCount = CompletionsByRule.Length;

            _requiresCompile = false;
        }

        public void SetToState(LexDfaState toState)
        {
            //Set the to state
            ToState = toState;

            //Update the state of the paths too!
            foreach (var productionPath in ProductionPaths)
            {
                productionPath.State = ToState;
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
}
