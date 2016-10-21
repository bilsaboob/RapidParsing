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
        
        private RapidTable<ISymbol, LexDfaStateTransition> Transitions { get; set; }
        private RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition> TermTransitions { get; set; }
        
        public int StateId { get; set; }

        public LexDfaState(LexDfa dfa)
        {
            _dfa = dfa;

            Transitions = new RapidTable<ISymbol, LexDfaStateTransition>();
            TermTransitions = new RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition>();
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
            var transition = TermTransitions.AddOrGetExisting(terminalSymbol, () => new LexDfaStateTermTransition(terminalSymbol));

            //Add the production state to the transition
            transition.AddProductionState(productionState);

            _requiresCompile = true;

            return transition;
        }

        private LexDfaStateTransition AddRuleSymbolTransition(ILexPatternRuleRefSymbol ruleSymbol, LexDfaProductionState productionState)
        {
            _requiresCompile = true;
            return null;
        }

        public LexDfaStateTermTransition[] GetTermTransitions()
        {
            return TermTransitions.Values;
        }

        public void Compile()
        {
            if(!_requiresCompile)
                return;

            //Compile each of the transitions, these will prepare a new state automatically
            foreach (var transition in GetTermTransitions())
            {
                transition.Compile(_dfa);
            }

            _requiresCompile = false;
        }

        public void AddPrediction(ISymbol symbol, IRule rule, LexDfaProductionState productionState)
        {
            //Add a prediction info?
        }

        public void AddCompletion(ISymbol symbol, LexDfaProductionState productionState)
        {
            //Add a completion info?

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
        private CachingRapidList<LexDfaProductionPath> _productionPaths;
        private CachingRapidList<LexDfaProductionState> _productionStates;
        private CachingRapidList<LexDfaProductionState> _completionProductionStates;
        private CachingRapidList<LexDfaStateCompletion> _completionsByRule;

        private bool _requiresCompile;

        public LexDfaStateTransition(ILexPatternSymbol transSymbol)
        {
            TransitionSymbol = transSymbol;
            _productionPaths = new CachingRapidList<LexDfaProductionPath>();
            _productionStates = new CachingRapidList<LexDfaProductionState>();
            _completionProductionStates = new CachingRapidList<LexDfaProductionState>();
            _completionsByRule = new CachingRapidList<LexDfaStateCompletion>();
           _requiresCompile = true;
        }

        public ILexPatternSymbol TransitionSymbol { get; private set; }
        public LexDfaState ToState { get; private set; }

        public int ProductionStatesCount { get { return _productionStates.Count; } }
        public LexDfaProductionState[] ProductionStates { get { return _productionStates.AsArray; } }

        public int CompletionsByRuleCount { get { return _completionsByRule.Count; } }
        public LexDfaStateCompletion[] CompletionsByRule { get { return _completionsByRule.AsArray; } }

        public void AddProductionState(LexDfaProductionState productionState)
        {
            //Add the production path... to set the state whenever transition is finished
            var productionPath = productionState.Path;
            _productionPaths.Add(productionPath);
            productionPath.Transition = this;

            //We are only interested in the production state...
            _productionStates.Add(productionState);

            //If the production is at the end, then we have a completion too!
            if (productionPath.IsAtEnd)
            {
                _completionProductionStates.Add(productionState);
            }

            //We have new productions... need to recompile later!
            _requiresCompile = true;
        }

        public void Compile(LexDfa dfa)
        {
            if(!_requiresCompile)
                return;

            //Ensure we have a ToState
            EnsureToState(dfa);

            //Build the completion by rule
            CompileCompletionsByRule();

            //Finally refresh the production paths state
            RefreshProductionPathsState();

            _requiresCompile = false;
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

        private void EnsureToState(LexDfa dfa)
        {
            //Ensure there is a to state, but reuse it if exists!
            if (ToState == null)
            {
                ToState = dfa.CreateState();
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
        public LexDfaStateTermTransition(ILexPatternTerminalSymbol termSymbol)
            : base(termSymbol)
        {
            TransitionTermSymbol = termSymbol;
        }

        public ILexPatternTerminalSymbol TransitionTermSymbol { get; private set; }
    }
}
