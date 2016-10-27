using System;
using System.Collections.Generic;
using RapidPliant.Automata;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    public class DfaGraph : Graph<DfaState, DfaState, DfaTransition>
    {
        public DfaGraph(DfaState startState)
            : base(startState, startState)
        {
        }

        protected override DfaState GetTransitionToState(DfaTransition transition)
        {
            return transition.ToState;
        }

        protected override IEnumerable<DfaTransition> GetStateTransitions(DfaState state)
        {
            return state.Transitions;
        }
    }

    public class DfaState : GraphStateBase<DfaState>
    {
        private List<DfaTransition> _transitions;
        
        public DfaState()
        {
            _transitions = ReusableList<DfaTransition>.GetAndClear();
        }
        
        public bool IsFinal { get; set; }

        public IReadOnlyList<DfaTransition> Transitions { get { return _transitions; } }

        public void AddTransition(DfaTransition transition)
        {
            _transitions.Add(transition);
        }

        public override void Dispose()
        {
            if (_transitions != null)
            {
                _transitions.ClearAndFree();
            }
        }
    }

    public class DfaTransition : IDisposable
    {
        private HashSet<ITerminal> _terminals;
        private Dictionary<IExpr, DfaCompletion> _completions;

        public DfaTransition(Interval interval, IEnumerable<NfaTransition> nfaTransitons, IEnumerable<NfaTransition> nfaFinalTransitions, DfaState toState)
        {
            NfaTransitions = nfaTransitons;
            NfaFinalTransitions = nfaFinalTransitions;

            Interval = interval;
            ToState = toState;

            _terminals = ReusableHashSet<ITerminal>.GetAndClear();
            _completions = ReusableDictionary<IExpr, DfaCompletion>.GetAndClear();

            CollectTransitionInfo();
        }
        
        public DfaState ToState { get; private set; }

        public Interval Interval { get; private set; }

        public IEnumerable<ITerminal> Terminals { get; private set; }

        public IEnumerable<NfaTransition> NfaTransitions { get; private set; }
        public IEnumerable<NfaTransition> NfaFinalTransitions { get; private set; }

        public IEnumerable<DfaCompletion> CompletionsByExpression { get { return _completions.Values; } }

        private void CollectTransitionInfo()
        {
            if (NfaTransitions != null)
            {
                foreach (var nfaTransition in NfaTransitions)
                {
                    //Collect the terminals
                    var terminalTransition = nfaTransition as TerminalNfaTransition;
                    if (terminalTransition != null)
                    {
                        _terminals.Add(terminalTransition.Terminal);
                    }
                }
            }

            if (NfaFinalTransitions != null)
            {
                foreach (var nfaFinalTransition in NfaFinalTransitions)
                {
                    CollectCompletionInfo(nfaFinalTransition);
                }
            }
        }

        private void CollectCompletionInfo(NfaTransition nfaTransition)
        {
            var expr = nfaTransition.Expression;
            if(expr == null)
                return;

            DfaCompletion completion;
            if (!_completions.TryGetValue(expr, out completion))
            {
                completion = new DfaCompletion(this, expr);
                _completions[expr] = completion;
            }

            completion.AddNfaTransition(nfaTransition);
        }

        public void Dispose()
        {
            if (_terminals != null)
            {
                _terminals.ClearAndFree();
                _terminals = null;
            }

            if (_completions != null)
            {
                _completions.ClearAndFree();
                _completions = null;
            }
        }
    }

    public class DfaCompletion : IDisposable
    {
        private HashSet<NfaTransition> _nfaTransitions;

        public DfaCompletion(DfaTransition dfaTransition, IExpr expr)
        {
            DfaTransition = dfaTransition;
            Expression = expr;

            _nfaTransitions = ReusableHashSet<NfaTransition>.GetAndClear();
        }
        
        public DfaTransition DfaTransition { get; private set; }
        public IExpr Expression { get; private set; }
        public IEnumerable<NfaTransition> NfaTransitions { get { return _nfaTransitions; } }

        public void AddNfaTransition(NfaTransition nfaTransition)
        {
            _nfaTransitions.Add(nfaTransition);
        }

        public void Dispose()
        {
            if (_nfaTransitions != null)
            {
                _nfaTransitions.ClearAndFree();
                _nfaTransitions = null;
            }
        }
    }
}
