using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Util;
using RapidPliant.Runtime.Earley.Lex.Util;
using RapidPliant.Runtime.Util;

namespace RapidPliant.Runtime.Earley.Lex
{
    public interface ILexExpression
    {
        List<ILexProduction> Productions { get; set; }

        bool IsNullable(ILexSymbol lexSymbol);
    }

    public enum LexSymbolType
    {
        LexChar,
        LexExpression
    }

    public interface ILexSymbol
    {
        LexSymbolType SymbolType { get; }
    }

    public interface ILexCharSymbol : ILexSymbol
    {
    }

    public interface ILexExpressionSymbol : ILexSymbol
    {
        ILexExpression LexExpr { get; set; }
    }
    
    public interface ILexProduction
    {
        ILexExpression LeftHandSide { get; }

        List<ILexSymbol> RightHandSide { get; }

        bool IsEmpty { get; }
    }

    public class LexDfa
    {
        private LexDfaStatesProcessQueue _dfaStatesToProcess;
        
        public LexDfa(ILexExpression expr)
        {
            LexExpr = expr;

            _dfaStatesToProcess = new LexDfaStatesProcessQueue();

            Build();
        }
        
        public ILexExpression LexExpr { get; private set; }

        public LexDfaStatesCollection States { get; private set; }

        public LexDfaState StartState { get; set; }

        private void Build()
        {
            var startDfaProductions = Initialize(LexExpr);   
            StartState = GetOrAddState(startDfaProductions);

            ProcessStates();
        }

        private void ProcessStates()
        {
            while (!_dfaStatesToProcess.IsEmpty)
            {
                var dfaState = _dfaStatesToProcess.Dequeue();

                ProcessTransitions(dfaState);

                // capture the predictions for the frame
                var nonLambdaKernelStates = GetLambdaKernelStates(dfaState);

                // if no predictions, continue
                if (nonLambdaKernelStates.Count == 0)
                    continue;

                // assign the null transition
                // only process symbols on the null frame if it is new
                LexDfaState nullTransitionDfaState;
                if (!TryGetOrAddState(nonLambdaKernelStates, out nullTransitionDfaState))
                    ProcessTransitions(nullTransitionDfaState);

                dfaState.NullTransition = new LexDfaStateTransition(nullTransitionDfaState);
            }
        }

        private void ProcessTransitions(LexDfaState dfaState)
        {
            var dfaProductions = dfaState.Productions;
            var dfaStateTransitions = new Dictionary<ILexSymbol, LexDfaProductionsCollection>();

            LexDfaProductionsCollection symbolTransitionDfaProductions;
            for (var i = 0; i < dfaProductions.Length; ++i)
            {
                var dfaProduction = dfaProductions[i];
                if (dfaProduction.IsComplete)
                {
                    //The production is complete... add to completions?
                    continue;
                }

                var position = dfaProduction.Position;
                var lexProduction = dfaProduction.Production;
                var postDotSymbol = lexProduction.RightHandSide[position];

                //Get the state to which we will transition!
                if(!dfaStateTransitions.TryGetValue(postDotSymbol, out symbolTransitionDfaProductions))
                {
                    symbolTransitionDfaProductions = new LexDfaProductionsCollection();
                    dfaStateTransitions[postDotSymbol] = symbolTransitionDfaProductions;
                }
                
                //Prepare the next dfa production!
                var nextDfaProduction = new LexDfaProduction(lexProduction, position + 1);
                //Add the next dfa production!
                symbolTransitionDfaProductions.AddIfNotExists(nextDfaProduction);
            }

            foreach (var symbol in dfaStateTransitions.Keys)
            {
                var dfaStateTransitionDfaProductions = dfaStateTransitions[symbol];
                var closure = GetNonLambdaKernelStates(dfaStateTransitionDfaProductions.ToArray());
                var toDfaState = GetOrAddState(closure);
                dfaState.AddTransistion(symbol, toDfaState);
            }
        }

        private LexDfaProductionsCollection Initialize(ILexExpression expr)
        {
            var startDfaProductions = new LexDfaProductionsCollection();

            var startProductions = expr.Productions;
            for (int i = 0; i < startProductions.Count; i++)
            {
                startDfaProductions.AddIfNotExists(new LexDfaProduction(startProductions[i], 0));
            }

            var closure = GetNonLambdaKernelStates(startDfaProductions.ToArray());
            return closure;
        }

        private LexDfaProductionsCollection GetNonLambdaKernelStates(LexDfaProduction[] dfaProductions)
        {
            var productionsToProcess = new LexDfaProductionsProcessQueue();
            var closure = new LexDfaProductionsCollection();

            for (var i = 0; i < dfaProductions.Length; ++i)
            {
                var dfaProduction = dfaProductions[i];
                if (closure.AddIfNotExists(dfaProduction))
                {
                    productionsToProcess.Enqueue(dfaProduction);
                }
            }
            
            while (!productionsToProcess.IsEmpty)
            {
                var dfaProduction = productionsToProcess.Dequeue();

                if (dfaProduction.IsComplete)
                    continue;
                
                var production = dfaProduction.Production;
                var productionRhs = production.RightHandSide;

                for (var i = dfaProduction.Position; i < productionRhs.Count; i++)
                {
                    var postDotSymbol = productionRhs[i];
                    if (postDotSymbol.SymbolType != LexSymbolType.LexExpression)
                        break;

                    var lexExprSymbol = postDotSymbol as ILexExpressionSymbol;
                    if (!IsNullable(lexExprSymbol))
                        break;

                    var nextDfaProduction = new LexDfaProduction(production, i + 1);
                    if (closure.AddIfNotExists(nextDfaProduction))
                    {
                        productionsToProcess.Enqueue(nextDfaProduction);
                    }
                }
            }
            
            return closure;
        }
        

        private LexDfaProductionsCollection GetLambdaKernelStates(LexDfaState dfaState)
        {
            var productionsToProcess = new LexDfaProductionsProcessQueue();
            var closure = new LexDfaProductionsCollection();

            var dfaProductions = dfaState.Productions;

            for (var i = 0; i < dfaProductions.Length; ++i)
            {
                var dfaProduction = dfaProductions[i];
                if (closure.AddIfNotExists(dfaProduction))
                {
                    productionsToProcess.Enqueue(dfaProduction);
                }
            }

            while (!productionsToProcess.IsEmpty)
            {
                var dfaProduction = productionsToProcess.Dequeue();

                if (dfaProduction.IsComplete)
                    continue;

                var production = dfaProduction.Production;
                var productionRhs = production.RightHandSide;
                var position = dfaProduction.Position;

                var postDotSymbol = productionRhs[position];
                if (postDotSymbol.SymbolType != LexSymbolType.LexExpression)
                    continue;

                var lexExprSymbol = postDotSymbol as ILexExpressionSymbol;
                if (IsNullable(lexExprSymbol))
                {
                    var nextDfaProduction = new LexDfaProduction(production, position + 1);
                    if (!dfaState.ContainsProduction(nextDfaProduction))
                    {
                        if (closure.AddIfNotExists(nextDfaProduction))
                        {
                            if (!nextDfaProduction.IsComplete)
                            {
                                productionsToProcess.Enqueue(nextDfaProduction);
                            }
                        }
                    }
                }

                var lexExpr = lexExprSymbol.LexExpr;
                var predictedLexProductions = lexExpr.Productions;
                for (var i = 0; i < predictedLexProductions.Count; i++)
                {
                    var predictedLexProduction = predictedLexProductions[i];
                    var predictedDfaProduction = new LexDfaProduction(predictedLexProduction, 0);
                    if (!dfaState.ContainsProduction(predictedDfaProduction))
                    {
                        if (closure.AddIfNotExists(predictedDfaProduction))
                        {
                            if (!predictedDfaProduction.IsComplete)
                            {
                                productionsToProcess.Enqueue(predictedDfaProduction);
                            }
                        }
                    }
                }
            }

            return closure;
        }

        #region helpers

        private void EnqueueStateToProcess(LexDfaState lexDfaState)
        {
        }

        private LexDfaState GetOrAddState(LexDfaProductionsCollection dfaProductions)
        {
            var newDfaState = new LexDfaState(dfaProductions);

            //Get existing or add a new one!
            LexDfaState resultDfaState;
            if (!States.TryGetOrAdd(newDfaState, out resultDfaState))
            {
                EnqueueStateToProcess(resultDfaState);
            }

            return resultDfaState;
        }

        private bool TryGetOrAddState(LexDfaProductionsCollection dfaProductions, out LexDfaState outDfaState)
        {
            var newDfaState = new LexDfaState(dfaProductions);

            //Get existing or add a new one!
            if (!States.TryGetOrAdd(newDfaState, out outDfaState))
            {
                EnqueueStateToProcess(outDfaState);
                return false;
            }
            
            return true;
        }

        private bool IsNullable(ILexSymbol lexSymbol)
        {
            return LexExpr.IsNullable(lexSymbol);
        }

        #endregion
    }

    public class LexDfaStateTransition
    {
        public LexDfaStateTransition(LexDfaState toDfaState)
        {
            ToState = toDfaState;
        }

        public LexDfaState ToState { get; set; }
    }

    public class LexDfaState
    {
        private int _hashCode;
        private LexDfaProductionsCollection _productions;

        public LexDfaState(LexDfaProductionsCollection productions)
        {
            _productions = productions;

            Productions = _productions.ToArray();

            _hashCode = HashCode.Compute(Productions);
        }

        public LexDfaProduction[] Productions { get; private set; }

        public LexDfaTransitionsCollection Transitions { get; private set; }

        public LexDfaStateTransition NullTransition { get; set; }

        public void AddTransistion(ILexSymbol symbol, LexDfaState toDfaState)
        {
            //Add transition to the state for the specified symbol!
        }

        public bool ContainsProduction(LexDfaProduction lexDfaProduction)
        {
            return _productions.Contains(lexDfaProduction);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var otherState = obj as LexDfaState;
            if (ReferenceEquals(otherState, null))
                return false;

            for (var i = 0; i < Productions.Length; ++i)
            {
                if (!otherState.ContainsProduction(Productions[i]))
                    return false;
            }

            return base.Equals(obj);
        }

        public LexDfaStateTransition GetTransitionForChar(char c)
        {
            return null;
        }
    }

    public class LexDfaProduction
    {
        public LexDfaProduction(ILexProduction production, int position)
        {
            Production = production;
            Position = position;

            if (Position == production.RightHandSide.Count)
                IsComplete = true;
        }

        public ILexProduction Production { get; set; }

        public int Position { get; set; }

        public bool IsComplete { get; set; }
    }

}
