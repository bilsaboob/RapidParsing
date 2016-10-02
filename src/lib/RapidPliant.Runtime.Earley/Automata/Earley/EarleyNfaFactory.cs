using System;
using System.Collections.Generic;
using RapidPliant.Grammar;

namespace RapidPliant.Runtime.Earley.Automata.Earley
{
    public class EarleyNfaFactory
    {
        public EarleyNfa CreateNfaFromExpression(IExpr expr)
        {
            var nfa = Build(expr, true);
            return nfa;
        }

        private EarleyNfa Build(IExpr expr, bool expandRules = false)
        {
            var groupExpr = expr as IGroupExpr;
            if (groupExpr != null)
            {
                var altExpr = groupExpr as IAlterationExpr;
                if (altExpr != null)
                {
                    return BuildForAlteration(altExpr);
                }

                var prodExpr = groupExpr as IProductionExpr;
                if (prodExpr != null)
                {
                    return BuildForProduction(prodExpr);
                }

                throw new Exception($"Unhandled group expression of type '{expr.GetType().Name}'");
            }
            
            /*var nullExpr = expr as INullExpr;
            if (nullExpr != null)
            {
                //Don't add the null itself... just make the previous state optional!
                fromState.MakeOptional();
                return fromState;
            }*/

            var lexExpr = expr as ILexExpr;
            if (lexExpr != null)
            {
                return BuildForLex(lexExpr);
            }

            var ruleExpr = expr as IRuleExpr;
            if (ruleExpr != null)
            {
                return BuildForRule(ruleExpr);
            }

            return null;
        }

        private EarleyNfa BuildForRule(IRuleExpr ruleExpr)
        {
            var nfa = new EarleyNfa();
            nfa.Start.AddRuleTransitionTo(ruleExpr, nfa.End);
            return nfa;
        }

        private EarleyNfa BuildForLex(ILexExpr lexExpr)
        {
            var nfa = new EarleyNfa();
            nfa.Start.AddLexTransitionTo(lexExpr, nfa.End);
            return nfa;
        }

        private EarleyNfa BuildForProduction(IProductionExpr production)
        {
            //Attach each expression sequentially one after another
            var prodNfa = new EarleyNfa();
            prodNfa.Start.AddNullTransitionTo(prodNfa.End);

            var nfa = prodNfa;
            foreach (var expr in production.Expressions)
            {
                var exprNfa = Build(expr);
                nfa.ConcatWith(exprNfa);
                nfa = exprNfa;
            }

            return prodNfa;
        }

        private EarleyNfa BuildForAlteration(IAlterationExpr alteration)
        {
            var altNfa = new EarleyNfa();
            
            //Attach each expression as another branch in the tree
            foreach (var expr in alteration.Expressions)
            {
                var exprNfa = Build(expr);

                altNfa.Start.AddNullTransitionTo(exprNfa.Start);
                exprNfa.End.AddNullTransitionTo(altNfa.End);
            }
            
            return altNfa;
        }

        public EarleyNfa CreateNfaFromMany(IEnumerable<EarleyNfa> nfas)
        {
            var altNfa = new EarleyNfa();

            //Attach each expression as another branch in the tree
            foreach (var nfa in nfas)
            {
                altNfa.Start.AddNullTransitionTo(nfa.Start);
                nfa.End.AddNullTransitionTo(altNfa.End);
            }

            return altNfa;
        }
    }
}