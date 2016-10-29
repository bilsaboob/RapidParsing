using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    public interface INfaBuilder
    {
        INfa Create(IEnumerable<IExpr> expressions);
        INfa Create(IExpr expr);
    }

    public abstract class NfaBuilder : INfaBuilder, IDisposable
    {
        #region Create

        public INfa Create(IEnumerable<IExpr> expressions)
        {
            //Prepare the build context
            return BuildForAlterationExpression(null, expressions);
        }

        public INfa Create(IExpr expr)
        {
            return BuildForExpression(expr);
        }

        #endregion

        #region Build

        private INfa BuildForExpression(IExpr expr)
        {
            var isLeaf = !expr.IsGroup;

            INfa exprNfa;

            if (isLeaf)
            {
                exprNfa = BuildForLeafExpression(expr);
            }
            else
            {
                exprNfa = BuildForGroupExpression(expr);
            }

            if (expr.HasOptions)
            {
                var exprOpt = expr.Options;
                if (exprOpt.IsMany)
                {
                    if (exprOpt.IsOptional)
                    {
                        exprNfa = ZeroOrMore(exprNfa);
                    }
                    else
                    {
                        exprNfa = OneOrMore(exprNfa);
                    }
                }
                else if (exprOpt.IsOptional)
                {
                    exprNfa = Optional(exprNfa);
                }
            }

            return exprNfa;
        }

        protected virtual INfa BuildForLeafExpression(IExpr expr)
        {
            throw new Exception($"Unhandled leaf expression '{expr.ToString()}' of type '{expr.GetType().Name}'!");
        }

        protected INfa BuildForGroupExpression(IExpr expr)
        {
            var isProduction = expr.IsProduction;
            var isAlteration = expr.IsAlteration;

            if (isProduction)
            {
                return BuildForProductionExpression(expr);
            }
            else if (isAlteration)
            {
                return BuildForAlterationExpression(expr);
            }
            else
            {
                throw new Exception($"Invalid group expression '{expr.ToString()}' - should be either Production or Alteration!");
            }
        }

        protected INfa BuildForProductionExpression(IExpr expr)
        {
            return BuildForProductionExpression(expr, expr.Expressions);
        }

        protected INfa BuildForProductionExpression(IExpr productionExpr, IEnumerable<IExpr> expressions)
        {
            var nfas = new List<INfa>();

            foreach (var expr in expressions)
            {
                var nfa = BuildForExpression(expr);
                nfas.Add(nfa);
            }

            return And(productionExpr, nfas);
        }

        protected INfa BuildForAlterationExpression(IExpr expr)
        {
            return BuildForAlterationExpression(expr, expr.Expressions);
        }

        protected INfa BuildForAlterationExpression(IExpr alterationExpr, IEnumerable<IExpr> expressions)
        {
            var nfas = new List<INfa>();

            foreach (var expr in expressions)
            {
                var nfa = BuildForExpression(expr);
                nfas.Add(nfa);
            }

            return Or(alterationExpr, nfas);
        }

        #endregion

        #region Operation helpers
        protected INfa Or(IExpr orExpr, params INfa[] nfas)
        {
            return Or(orExpr, nfas.AsEnumerable());
        }

        protected INfa Or(IExpr orExpr, IEnumerable<INfa> nfas)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            var orExpressions = new List<IExpr>();

            foreach (var nfa in nfas)
            {
                orExpressions.Add(nfa.Expression);

                var startToOtherStart = CreateOpenOrTransition(start, nfa.Start, orExpr, nfa.Expression);
                AssociateTransitionWithExpression(startToOtherStart, nfa.Expression);
                start.AddTransition(startToOtherStart);

                var otherEndToNewEnd = CreateCloseOrTransition(nfa.End, end, orExpr, nfa.Expression);
                AssociateTransitionWithExpression(otherEndToNewEnd, nfa.Expression);
                nfa.End.AddTransition(otherEndToNewEnd);
            }

            var orNfa = CreateOrNfa(start, end, orExpr, orExpressions);
            AssociateNfaWithExpression(orNfa, orExpr);
            return orNfa;
        }

        protected INfa And(IExpr andExpr, params INfa[] nfas)
        {
            return And(andExpr, nfas.AsEnumerable());
        }

        protected INfa And(IExpr andExpr, IEnumerable<INfa> nfas)
        {
            INfaState start = null;
            INfaState end = null;

            INfa prevNfa = null;
            foreach (var nfa in nfas)
            {
                if (start == null)
                {
                    start = nfa.Start;
                }

                if (prevNfa != null)
                {
                    var andJoinTransition = CreateAndJoinTransition(prevNfa.End, nfa.Start, andExpr, nfa.Expression);
                    AssociateTransitionWithExpression(andJoinTransition, nfa.Expression);
                    prevNfa.End.AddTransition(andJoinTransition);
                }

                end = nfa.End;

                prevNfa = nfa;
            }

            var andNfa = CreateAndNfa(start, end, andExpr);
            AssociateNfaWithExpression(andNfa, andExpr);

            return andNfa;
        }

        protected INfa ZeroOrMore(INfa nfa)
        {
            var start = CreateNfaState();
            var nullToNfaStart = CreateRepeatStartTransition(start, nfa.Start, nfa.Expression);
            AssociateTransitionWithExpression(nullToNfaStart, nfa.Expression);

            start.AddTransition(nullToNfaStart);
            nfa.End.AddTransition(nullToNfaStart);

            var end = CreateNfaState();
            var nullToNewEnd = CreateRepeatEndTransition(start, end, nfa.Expression);
            AssociateTransitionWithExpression(nullToNewEnd, nfa.Expression);

            start.AddTransition(nullToNewEnd);
            nfa.End.AddTransition(nullToNewEnd);

            var repeatNfa = CreateRepeatNfa(start, end, nfa.Expression);
            AssociateNfaWithExpression(repeatNfa, nfa.Expression);

            return repeatNfa;
        }

        protected INfa OneOrMore(INfa nfa)
        {
            var end = CreateNfaState();

            var endToStart = CreateRepeatStartTransition(nfa.End, nfa.Start, nfa.Expression);
            AssociateTransitionWithExpression(endToStart, nfa.Expression);
            nfa.End.AddTransition(endToStart);

            var endToNewEnd = CreateRepeatEndTransition(nfa.End, end, nfa.Expression);
            AssociateTransitionWithExpression(endToNewEnd, nfa.Expression);
            nfa.End.AddTransition(endToNewEnd);

            var repeatNfa = CreateRepeatNfa(nfa.Start, end, nfa.Expression);
            AssociateNfaWithExpression(repeatNfa, nfa.Expression);

            return repeatNfa;
        }

        protected INfa Optional(INfa nfa)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            //New "optional path" opening transition to the initial nfa => start for normal path
            var newStartToStart = CreateOptionalStartTransition(start, nfa.Start, nfa.Expression);
            AssociateTransitionWithExpression(newStartToStart, nfa.Expression);
            start.AddTransition(newStartToStart);

            //New "optional path" optional skip to end transition => skip over transition
            var optionalEndToNewEnd = CreateOptionalEndTransition(start, end, nfa.Expression);
            AssociateTransitionWithExpression(optionalEndToNewEnd, nfa.Expression);
            start.AddTransition(optionalEndToNewEnd);

            //Initial nfa join to new end => this is the "normal path" taken
            var optionalEndToEnd = CreateOptionalEndTransition(nfa.End, end, nfa.Expression);
            AssociateTransitionWithExpression(optionalEndToEnd, nfa.Expression);
            nfa.End.AddTransition(optionalEndToEnd);

            var optionalNfa = CreateOptionalNfa(start, end, nfa.Expression);
            AssociateNfaWithExpression(optionalNfa, nfa.Expression);

            return optionalNfa;
        }

        protected INfa Empty(IExpr expr)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            var startToEnd = CreateEmptyStartToEndTransition(start, end, expr);
            AssociateTransitionWithExpression(startToEnd, expr);
            start.AddTransition(startToEnd);

            var emptyNfa = CreateEmptyNfa(start, end, expr);
            AssociateNfaWithExpression(emptyNfa, expr);
            return emptyNfa;
        }


        #endregion

        #region Helpers

        protected virtual INfa CreateNfa(INfaState start, INfaState end, IExpr forExpression)
        {
            return new Nfa(start, end);
        }

        protected virtual INfa CreateEmptyNfa(INfaState start, INfaState end, IExpr forExpression)
        {
            return new Nfa(start, end);
        }

        protected virtual INfa CreateOrNfa(INfaState start, INfaState end, IExpr orExpr, IEnumerable<IExpr> forExpressions)
        {
            return new Nfa(start, end);
        }

        protected virtual INfa CreateAndNfa(INfaState start, INfaState end, IExpr forExpression)
        {
            return new Nfa(start, end);
        }

        protected virtual INfa CreateRepeatNfa(INfaState start, INfaState end, IExpr forExpression)
        {
            return new Nfa(start, end);
        }

        protected virtual INfa CreateOptionalNfa(INfaState start, INfaState end, IExpr forExpression)
        {
            return new Nfa(start, end);
        }

        protected virtual INfaState CreateNfaState()
        {
            return new NfaState();
        }

        protected abstract INfaTransition CreateNullTransition(INfaState fromState, INfaState toState, IExpr forExpression);

        protected virtual INfaTransition CreateOpenOrTransition(INfaState fromState, INfaState toState, IExpr orExpr, IExpr forExpression)
        {
            return CreateNullTransition(fromState, toState, forExpression);
        }
        
        protected virtual INfaTransition CreateCloseOrTransition(INfaState fromState, INfaState toState, IExpr orExpr, IExpr forExpression)
        {
            return CreateNullTransition(fromState, toState, forExpression);
        }

        protected virtual INfaTransition CreateAndJoinTransition(INfaState fromState, INfaState toState, IExpr andExpr, IExpr forExpression)
        {
            return CreateNullTransition(fromState, toState, forExpression);
        }

        protected virtual INfaTransition CreateRepeatStartTransition(INfaState fromState, INfaState toState, IExpr forExpression)
        {
            return CreateNullTransition(fromState, toState, forExpression);
        }

        protected virtual INfaTransition CreateRepeatEndTransition(INfaState fromState, INfaState toState, IExpr forExpression)
        {
            return CreateNullTransition(fromState, toState, forExpression);
        }

        protected virtual INfaTransition CreateOptionalStartTransition(INfaState fromState, INfaState toState, IExpr forExpression)
        {
            return CreateNullTransition(fromState, toState, forExpression);
        }

        protected virtual INfaTransition CreateOptionalEndTransition(INfaState fromState, INfaState toState, IExpr forExpression)
        {
            return CreateNullTransition(fromState, toState, forExpression);
        }

        protected virtual INfaTransition CreateEmptyStartToEndTransition(INfaState fromState, INfaState toState, IExpr forExpression)
        {
            return CreateNullTransition(fromState, toState, forExpression);
        }

        protected virtual void AssociateNfaWithExpression(INfa nfa, IExpr expression)
        {
            if (nfa.Expression == null)
                nfa.Expression = expression;
        }
        protected virtual void AssociateTransitionWithExpression(INfaTransition transition, IExpr expression)
        {
            if (transition.Expression == null)
                transition.Expression = expression;
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
