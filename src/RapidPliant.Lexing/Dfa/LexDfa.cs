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

        private bool _requiresCompile;
        private bool _requiresBuild;

        private RapidTable<ISymbol, LexDfaStateTransition> Transitions { get; set; }
        private RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition> TermTransitions { get; set; }
        
        public int StateId { get; set; }

        public LexDfaState(LexDfa dfa)
        {
            _dfa = dfa;
            
            Transitions = new RapidTable<ISymbol, LexDfaStateTransition>();
            TermTransitions = new RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition>();

            _requiresBuild = true;
            _requiresCompile = true;
        }

        public LexDfaStateTransition AddSymbolTransition(ISymbol symbol, LexDfaProductionState productionState)
        {
            var termSymbol = symbol as ILexPatternTerminalSymbol;
            if (termSymbol != null)
            {
                return AddTerminalSymbolTransition(termSymbol, productionState);
            }

            var ruleRefSymbol = symbol as ILexPatternRuleRefSymbol;
            if (ruleRefSymbol != null)
            {
                return AddRuleSymbolTransition(ruleRefSymbol, productionState);
            }

            return null;
        }

        private LexDfaStateTransition AddTerminalSymbolTransition(ILexPatternTerminalSymbol terminalSymbol, LexDfaProductionState productionState)
        {
            //Either get the existing or add a new one
            var transition = TermTransitions.AddOrGetExisting(terminalSymbol, () => new LexDfaStateTermTransition(_dfa, terminalSymbol) {IsNew = true});
            transition.AddTransitionProductionState(productionState);

            //If the transition was just created, we will need to rebuild
            if (transition.IsNew)
            {
                _requiresBuild = true;
                transition.IsNew = false;
            }

            //Always recompile
            _requiresCompile = true;

            return transition;
        }

        private LexDfaStateTransition AddRuleSymbolTransition(ILexPatternRuleRefSymbol ruleSymbol, LexDfaProductionState productionState)
        {
            _requiresCompile = true;
            _requiresBuild = true;

            return null;
        }

        public LexDfaStateTermTransition[] GetTermTransitions()
        {
            return TermTransitions.Values;
        }

        public void Build()
        {
            if(!_requiresBuild)
                return;
            _requiresBuild = false;

            foreach (var transition in GetTermTransitions())
            {
                transition.Build();
            }
        }

        public void Compile()
        {
            if(!_requiresCompile)
                return;
            _requiresCompile = false;

            //Compile the transitions and then the to states recursively!
            foreach (var transition in GetTermTransitions())
            {
                transition.Compile();
                
                var toState = transition.ToState;
                if (toState != null)
                {
                    toState.Compile();
                }
            }
        }

        public void AddPrediction(ISymbol symbol, IRule rule, LexDfaProductionState productionState)
        {
            //Add a prediction info?
        }

        public void AddCompletion(ISymbol symbol, LexDfaProductionState productionState)
        {
            var path = productionState.Path;
            var lastTransition = path.Transition;
            if (lastTransition != null)
            {
                lastTransition.AddCompletionProductionState(productionState);
            }
        }

        public void AddScan(ISymbol symbol, LexDfaProductionState productionState)
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
        private LexDfa _dfa;

        private CachingRapidList<LexDfaProductionPath> _productionPaths;
        private CachingRapidList<LexDfaProductionState> _productionStates;
        private CachingRapidList<LexDfaProductionState> _completionProductionStates;
        private CachingRapidList<LexDfaStateCompletion> _completionsByRule;

        private bool _requiresCompile;
        private bool _requiresBuild;

        public LexDfaStateTransition(LexDfa dfa, ILexPatternSymbol transSymbol)
        {
            TransitionSymbol = transSymbol;

            _dfa = dfa;

            _productionPaths = new CachingRapidList<LexDfaProductionPath>();
            _productionStates = new CachingRapidList<LexDfaProductionState>();
            _completionProductionStates = new CachingRapidList<LexDfaProductionState>();
            _completionsByRule = new CachingRapidList<LexDfaStateCompletion>();

            _requiresCompile = true;
            _requiresBuild = true;
        }

        public ILexPatternSymbol TransitionSymbol { get; private set; }
        public LexDfaState ToState { get; private set; }

        public int ProductionStatesCount { get { return _productionStates.Count; } }
        public LexDfaProductionState[] ProductionStates { get { return _productionStates.AsArray; } }

        public int CompletionsByRuleCount { get { return _completionsByRule.Count; } }
        public LexDfaStateCompletion[] CompletionsByRule { get { return _completionsByRule.AsArray; } }

        public bool IsNew { get; set; }

        public void AddTransitionProductionState(LexDfaProductionState productionState)
        {
            //Add the production path... to set the state whenever transition is finished
            var productionPath = productionState.Path;
            _productionPaths.Add(productionPath);
            productionPath.Transition = this;

            //We are only interested in the production state...
            _productionStates.Add(productionState);

            //If the production is at the end, then we have a completion too!
            if (productionState.IsAtEnd)
            {
                _completionProductionStates.Add(productionState);
            }

            //We have new productions... need to recompile later...
            _requiresCompile = true;
        }

        public void AddCompletionProductionState(LexDfaProductionState productionState)
        {
            _completionProductionStates.Add(productionState);
            _requiresCompile = true;
        }

        public void Build()
        {
            if(!_requiresBuild)
                return;
            _requiresBuild = false;

            //Ensure we have a ToState
            BuildToState();
        }

        public void Compile()
        {
            if(!_requiresCompile)
                return;
            _requiresCompile = false;

            //Ensure we have a ToState
            BuildToState();

            //Build the completion by rule
            CompileCompletionsByRule();

            //Finally refresh the production paths state
            RefreshProductionPathsState();
        }

        private void CompileCompletionsByRule()
        {
            var completionsByRule = new RapidTable<IRule, LexDfaStateCompletion>();

            //Compile the completions by rule
            if (_completionProductionStates.Count > 0)
            {
                foreach (var completionProductionState in _completionProductionStates.AsArray)
                {
                    var completedRule = completionProductionState.Production.LhsRule;
                    var completion = completionsByRule.AddOrGetExisting(completedRule, () => new LexDfaStateCompletion(completedRule));
                    completion.AddProductionState(completionProductionState);
                }
            }
            
            _completionsByRule.FromArray(completionsByRule.Values);
        }

        private void BuildToState()
        {
            //Ensure there is a to state, but reuse it if exists!
            if (ToState == null)
            {
                ToState = _dfa.CreateState();
            }
        }

        private void RefreshProductionPathsState()
        {
            //Update the state of each of the production paths
            foreach (var productionPath in _productionPaths)
            {
                //Set the new to state for the production path!
                productionPath.State = ToState;
            }
        }
    }

    public class LexDfaStateTermTransition : LexDfaStateTransition
    {
        public LexDfaStateTermTransition(LexDfa dfa, ILexPatternTerminalSymbol termSymbol)
            : base(dfa, termSymbol)
        {
            TransitionTermSymbol = termSymbol;
        }

        public ILexPatternTerminalSymbol TransitionTermSymbol { get; private set; }
    }
}
