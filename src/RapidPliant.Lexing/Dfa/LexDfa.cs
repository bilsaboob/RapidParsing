using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private RapidTable<ISymbol, LexDfaStateTransition> Transitions { get; set; }
        private RapidTable<ILexPatternTerminalSymbol, LexDfaStateTermTransition> TermTransitions { get; set; }

        private bool _hasCompiled;

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
                        var transition = state.AddSymbolTransition(symbol, activePath.Clone());
                        activePath.Transition = transition;
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
        private LexDfaState _state;
        private LexDfaProductionState _productionState;
        private LexDfaProductionPath _parentPath;

        public LexDfaProductionPath(IProduction production, LexDfaState state, LexDfaProductionPath parentPath = null)
        {
            _state = state;

            _production = production;
            _rhsIndex = 0;

            _rhsSymbolsCount = Production.RhsSymbolsCount;
            _rhsSymbols = Production.RhsSymbols;
            _rhsEndIndex = _rhsSymbolsCount - 1;

            _isAtEnd = _rhsIndex >= _rhsSymbolsCount;
            _symbol = _rhsSymbols[_rhsIndex];

            _parentPath = parentPath;
        }

        public LexDfaState State { get { return _state; } set { _state = value; } }

        public int SymbolIndex { get { return _rhsIndex; } }
        public IProduction Production { get { return _production; } }

        public bool IsAtEnd { get { return _isAtEnd; } }
        public ISymbol Symbol { get { return _symbol; } }

        public LexDfaProductionPath ParentPath { get { return _parentPath; } }
        public LexDfaStateTransition Transition { get; set; }

        public LexDfaProductionPath MoveToFirst()
        {
            MoveNext();
            return this;
        }

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
            if (_productionState == null)
            {
                _productionState = new LexDfaProductionState(this);
            }

            return _productionState;
        }

        public LexDfaProductionPath Clone()
        {
            LexDfaProductionPath parentPath = null;
            if (_parentPath != null)
            {
                parentPath = _parentPath.Clone();
            }
            var path = new LexDfaProductionPath(_production, _state, parentPath);
            path._rhsIndex = _rhsIndex;
            path._rhsEndIndex = _rhsEndIndex;
            path._isAtEnd = _isAtEnd;
            return path;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            const string dotStr = "\u25CF";

            sb.AppendFormat("{0}->", Production.LhsRule.ToString());

            var rhsCount = Production.RhsSymbolsCount;
            var rhs = Production.RhsSymbols;
            for (var i = 0; i < rhsCount; ++i)
            {
                var symbol = rhs[i];

                if (i > 0)
                    sb.Append(" ");

                if (i == SymbolIndex && !(IsAtEnd && i == rhsCount - 1))
                {
                    sb.AppendFormat("{0}{1}", dotStr, symbol.ToString());
                }
                else
                {
                    sb.AppendFormat("{0}", symbol.ToString());
                }
            }

            if (IsAtEnd)
            {
                sb.Append(dotStr);
            }

            return sb.ToString();
        }
    }
}
