using System;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    public abstract class NfaBuildContext<TNfa, TNfaState, TNfaTransition> : IDisposable
        where TNfa : Nfa<TNfa, TNfaState, TNfaTransition>
        where TNfaState : NfaState<TNfaState, TNfaTransition>
        where TNfaTransition : NfaTransition<TNfaState, TNfaTransition>
    {
        protected NfaBuildContext()
        {
        }

        #region Operation helpers
        public TNfa Or(IExpr orExpr, params TNfa[] nfas)
        {
            return Or(orExpr, nfas.AsEnumerable());
        }

        public TNfa Or(IExpr orExpr, IEnumerable<TNfa> nfas)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            var orExpressions = new List<IExpr>();

            foreach (var nfa in nfas)
            {
                orExpressions.Add(nfa.Expression);

                var startToOtherStart = CreateOpenOrTransition(start, nfa.Start, orExpr, nfa.Expression);
                AssociateTransitionWithExpression(startToOtherStart, nfa.Expression);
                start.AddTransistion(startToOtherStart);

                var otherEndToNewEnd = CreateCloseOrTransition(nfa.End, end, orExpr, nfa.Expression);
                AssociateTransitionWithExpression(otherEndToNewEnd, nfa.Expression);
                nfa.End.AddTransistion(otherEndToNewEnd);
            }

            var orNfa = CreateOrNfa(start, end, orExpr, orExpressions);
            AssociateNfaWithExpression(orNfa, orExpr);
            return orNfa;
        }
    
        public TNfa And(IExpr andExpr, params TNfa[] nfas)
        {
            return And(andExpr, nfas.AsEnumerable());
        }

        public TNfa And(IExpr andExpr, IEnumerable<TNfa> nfas)
        {
            IExpr startNfaExpression = null;

            TNfaState start = null;
            TNfaState end = null;

            TNfa prevNfa = null;
            foreach (var nfa in nfas)
            {
                if (start == null)
                {
                    startNfaExpression = nfa.Expression;
                    start = nfa.Start;
                }

                if (prevNfa != null)
                {
                    var andJoinTransition = CreateAndJoinTransition(prevNfa.End, nfa.Start, andExpr, nfa.Expression);
                    AssociateTransitionWithExpression(andJoinTransition, nfa.Expression);
                    prevNfa.End.AddTransistion(andJoinTransition);
                }

                end = nfa.End;

                prevNfa = nfa;
            }

            var andNfa = CreateAndNfa(start, end, andExpr);
            AssociateNfaWithExpression(andNfa, andExpr);

            return andNfa;
        }

        public TNfa ZeroOrMore(TNfa nfa)
        {
            var start = CreateNfaState();
            var nullToNfaStart = CreateRepeatStartTransition(start, nfa.Start, nfa.Expression);
            AssociateTransitionWithExpression(nullToNfaStart, nfa.Expression);

            start.AddTransistion(nullToNfaStart);
            nfa.End.AddTransistion(nullToNfaStart);

            var end = CreateNfaState();
            var nullToNewEnd = CreateRepeatEndTransition(start, end, nfa.Expression);
            AssociateTransitionWithExpression(nullToNewEnd, nfa.Expression);

            start.AddTransistion(nullToNewEnd);
            nfa.End.AddTransistion(nullToNewEnd);

            var repeatNfa = CreateRepeatNfa(start, end, nfa.Expression);
            AssociateNfaWithExpression(repeatNfa, nfa.Expression);

            return repeatNfa;
        }
        
        public TNfa OneOrMore(TNfa nfa)
        {
            var end = CreateNfaState();

            var endToStart = CreateRepeatStartTransition(nfa.End, nfa.Start, nfa.Expression);
            AssociateTransitionWithExpression(endToStart, nfa.Expression);
            nfa.End.AddTransistion(endToStart);

            var endToNewEnd = CreateRepeatEndTransition(nfa.End, end, nfa.Expression);
            AssociateTransitionWithExpression(endToNewEnd, nfa.Expression);
            nfa.End.AddTransistion(endToNewEnd);

            var repeatNfa = CreateRepeatNfa(nfa.Start, end, nfa.Expression);
            AssociateNfaWithExpression(repeatNfa, nfa.Expression);

            return repeatNfa;
        }

        public TNfa Optional(TNfa nfa)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            //New "optional path" opening transition to the initial nfa => start for normal path
            var newStartToStart = CreateOptionalStartTransition(start, nfa.Start, nfa.Expression);
            AssociateTransitionWithExpression(newStartToStart, nfa.Expression);
            start.AddTransistion(newStartToStart);

            //New "optional path" optional skip to end transition => skip over transition
            var optionalEndToNewEnd = CreateOptionalEndTransition(start, end, nfa.Expression);
            AssociateTransitionWithExpression(optionalEndToNewEnd, nfa.Expression);
            start.AddTransistion(optionalEndToNewEnd);

            //Initial nfa join to new end => this is the "normal path" taken
            var optionalEndToEnd = CreateOptionalEndTransition(nfa.End, end, nfa.Expression);
            AssociateTransitionWithExpression(optionalEndToEnd, nfa.Expression);
            nfa.End.AddTransistion(optionalEndToEnd);

            var optionalNfa = CreateOptionalNfa(start, end, nfa.Expression);
            AssociateNfaWithExpression(optionalNfa, nfa.Expression);

            return optionalNfa;
        }

        public TNfa Empty(IExpr expr)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            var startToEnd = CreateEmptyStartToEndTransition(start, end, expr);
            AssociateTransitionWithExpression(startToEnd, expr);
            start.AddTransistion(startToEnd);

            var emptyNfa = CreateEmptyNfa(start, end, expr);
            AssociateNfaWithExpression(emptyNfa, expr);
            return emptyNfa;
        }

        
        #endregion

        #region Helpers

        protected abstract TNfa CreateNfa(TNfaState start, TNfaState end, IExpr forExpression);
        protected abstract TNfa CreateEmptyNfa(TNfaState start, TNfaState end, IExpr forExpression);
        protected abstract TNfa CreateOrNfa(TNfaState start, TNfaState end, IExpr orExpr, IEnumerable<IExpr> forExpressions);
        protected abstract TNfa CreateAndNfa(TNfaState start, TNfaState end, IExpr forExpression);
        protected abstract TNfa CreateRepeatNfa(TNfaState start, TNfaState end, IExpr forExpression);
        protected abstract TNfa CreateOptionalNfa(TNfaState start, TNfaState end, IExpr forExpression);

        protected abstract TNfaState CreateNfaState();

        protected abstract TNfaTransition CreateOpenOrTransition(TNfaState fromState, TNfaState toState, IExpr orExpr, IExpr forExpression);
        protected abstract TNfaTransition CreateCloseOrTransition(TNfaState fromState, TNfaState toState, IExpr orExpr, IExpr forExpression);

        protected abstract TNfaTransition CreateAndJoinTransition(TNfaState fromState, TNfaState toState, IExpr andExpr, IExpr forExpression);

        protected abstract TNfaTransition CreateRepeatStartTransition(TNfaState fromState, TNfaState toState, IExpr forExpression);
        protected abstract TNfaTransition CreateRepeatEndTransition(TNfaState fromState, TNfaState toState, IExpr forExpression);

        protected abstract TNfaTransition CreateOptionalStartTransition(TNfaState fromState, TNfaState toState, IExpr forExpression);
        protected abstract TNfaTransition CreateOptionalEndTransition(TNfaState fromState, TNfaState toState, IExpr forExpression);

        protected abstract TNfaTransition CreateEmptyStartToEndTransition(TNfaState fromState, TNfaState toState, IExpr forExpression);

        protected virtual void AssociateNfaWithExpression(TNfa nfa, IExpr expression)
        {
            if(nfa.Expression == null)
                nfa.Expression = expression;
        }
        protected virtual void AssociateTransitionWithExpression(TNfaTransition transition, IExpr expression)
        {
            if(transition.Expression == null)
                transition.Expression = expression;
        }
        #endregion

        #region Dispose
        protected bool IsDisposed { get; set; }

        ~NfaBuildContext()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(false);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }
        #endregion
    }

    public abstract class NfaBuilder<TNfa, TNfaState, TNfaTransition, TNfaBuildContext> : IDisposable
        where TNfa : Nfa<TNfa, TNfaState, TNfaTransition>
        where TNfaState : NfaState<TNfaState, TNfaTransition>
        where TNfaTransition : NfaTransition<TNfaState, TNfaTransition>
        where TNfaBuildContext : NfaBuildContext<TNfa, TNfaState, TNfaTransition>, new()
    {
        #region Create

        public TNfa Create(IEnumerable<IExpr> expressions)
        {
            //Prepare the build context
            using (var buildContext = CreateBuildContext())
            {
                return BuildForAlterationExpression(buildContext, null, expressions);
            }
        }

        public TNfa Create(IExpr expr)
        {
            using (var buildContext = CreateBuildContext())
            {
                return BuildForExpression(buildContext, expr).WithExpression(expr);
            }
        }

        #endregion

        #region Build

        private TNfa BuildForExpression(TNfaBuildContext c, IExpr expr)
        {
            var isLeaf = !expr.IsGroup;

            TNfa exprNfa;

            if (isLeaf)
            {
                exprNfa = BuildForLeafExpression(c, expr);
            }
            else
            {
                exprNfa = BuildForGroupExpression(c, expr);
            }

            if (expr.HasOptions)
            {
                var exprOpt = expr.Options;
                if (exprOpt.IsMany)
                {
                    if (exprOpt.IsOptional)
                    {
                        exprNfa = c.ZeroOrMore(exprNfa);
                    }
                    else
                    {
                        exprNfa = c.OneOrMore(exprNfa);
                    }
                }
                else if (exprOpt.IsOptional)
                {
                    exprNfa = c.Optional(exprNfa);
                }
            }

            return exprNfa;
        }

        protected virtual TNfa BuildForLeafExpression(TNfaBuildContext c, IExpr expr)
        {
            throw new Exception($"Unhandled leaf expression '{expr.ToString()}' of type '{expr.GetType().Name}'!");
        }

        protected TNfa BuildForGroupExpression(TNfaBuildContext c, IExpr expr)
        {
            var isProduction = expr.IsProduction;
            var isAlteration = expr.IsAlteration;

            if (isProduction)
            {
                return BuildForProductionExpression(c, expr);
            }
            else if (isAlteration)
            {
                return BuildForAlterationExpression(c, expr);
            }
            else
            {
                throw new Exception($"Invalid group expression '{expr.ToString()}' - should be either Production or Alteration!");
            }
        }

        protected TNfa BuildForProductionExpression(TNfaBuildContext c, IExpr expr)
        {
            return BuildForProductionExpression(c, expr, expr.Expressions);
        }

        protected TNfa BuildForProductionExpression(TNfaBuildContext c, IExpr productionExpr, IEnumerable<IExpr> expressions)
        {
            var nfas = new List<TNfa>();

            foreach (var expr in expressions)
            {
                var nfa = BuildForExpression(c, expr);
                nfas.Add(nfa);
            }

            return c.And(productionExpr, nfas);
        }

        protected TNfa BuildForAlterationExpression(TNfaBuildContext c, IExpr expr)
        {
            return BuildForAlterationExpression(c, expr, expr.Expressions);
        }

        protected TNfa BuildForAlterationExpression(TNfaBuildContext c, IExpr alterationExpr, IEnumerable<IExpr> expressions)
        {
            var nfas = new List<TNfa>();

            foreach (var expr in expressions)
            {
                var nfa = BuildForExpression(c, expr);
                nfas.Add(nfa);
            }

            return c.Or(alterationExpr, nfas);
        }

        #endregion

        #region Helpers
        
        protected virtual TNfaBuildContext CreateBuildContext()
        {
            return new TNfaBuildContext();
        }
        #endregion

        #region Dispose
        protected bool IsDisposed { get; set; }

        ~NfaBuilder()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(false);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }
        #endregion
    }

}
