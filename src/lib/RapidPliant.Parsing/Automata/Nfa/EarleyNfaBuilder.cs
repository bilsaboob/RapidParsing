using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Nfa;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Parsing;
using RapidPliant.Grammar;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Parsing.Automata.Nfa
{
    public class ResolveRulesContext
    {
        private Dictionary<int, RuleDefResolveStackFrame> _activeResolveFrames;
        private RuleDefResolveStackFrame _activeFrame;

        public ResolveRulesContext()
        {
            _activeResolveFrames = new Dictionary<int, RuleDefResolveStackFrame>();
            _activeFrame = new RuleDefResolveStackFrame(null);
        }

        public void BeginResolvingRuleDef(IRuleDef ruleDef)
        {
            var id = ruleDef.Id;
            RuleDefResolveStackFrame ruleResolveFrame;
            if(_activeResolveFrames.TryGetValue(id, out ruleResolveFrame))
                throw new Exception($"Alreay building rule def '{ruleDef.Name}'!");

            ruleResolveFrame = new RuleDefResolveStackFrame(_activeFrame);
            _activeResolveFrames[id] = ruleResolveFrame;
            _activeFrame = ruleResolveFrame;
        }

        public bool IsResolvingForRuleDef(IRuleDef ruleDef)
        {
            var id = ruleDef.Id;
            RuleDefResolveStackFrame ruleResolveFrame;
            _activeResolveFrames.TryGetValue(id, out ruleResolveFrame);

            return ruleResolveFrame != null;
        }

        public INfa GetNfaForRuleDef(IRuleDef ruleDef)
        {
            if (_activeFrame == null)
                throw new Exception("No active rule resolve frame!");

            return _activeFrame.GetNfaForRuleDef(ruleDef);
        }

        public void SetNfaForRuleDef(IRuleDef ruleDef, INfa ruleNfa)
        {
            if(_activeFrame == null)
                throw new Exception("No active rule resolve frame!");

            _activeFrame.SetNfaForRuleDef(ruleDef, ruleNfa);
        }

        public void EndResolvingRuleDef(IRuleDef ruleDef)
        {
            var id = ruleDef.Id;
            var ruleResolveFrame = _activeResolveFrames[id];
            _activeFrame = null;
            if (ruleResolveFrame != null)
            {
                _activeFrame = ruleResolveFrame.PrevResolveFrame;
            }
            _activeResolveFrames[id] = null;
        }

        private class RuleDefResolveStackFrame
        {
            public Dictionary<int, INfa> _nfasByRuleDef;

            public RuleDefResolveStackFrame(RuleDefResolveStackFrame prevResolveFrame)
            {
                PrevResolveFrame = prevResolveFrame;

                _nfasByRuleDef = new Dictionary<int, INfa>();
            }

            public RuleDefResolveStackFrame PrevResolveFrame { get; private set; }

            public INfa GetNfaForRuleDef(IRuleDef ruleDef)
            {
                var id = ruleDef.Id;
                INfa ruleNfa;
                if (_nfasByRuleDef.TryGetValue(id, out ruleNfa))
                    return ruleNfa;

                if (PrevResolveFrame == null)
                    return null;

                return PrevResolveFrame.GetNfaForRuleDef(ruleDef);
            }

            public void SetNfaForRuleDef(IRuleDef ruleDef, INfa nfa)
            {
                var id = ruleDef.Id;
                INfa ruleNfa;
                if (_nfasByRuleDef.TryGetValue(id, out ruleNfa))
                {
                    throw new Exception($"NFA already exists for rule def '{ruleDef.Name}'!");
                }

                _nfasByRuleDef[id] = nfa;
            }

        }
    }

    public class EarleyNfaBuilder : NfaBuilder
    {
        public EarleyNfaBuilder()
        {
        }
        
        protected override INfa OnCreated(INfa nfa)
        {
            var c = new ResolveRulesContext();

            var firstTransitionWithExpr = nfa.Start.Transitions.FirstOrDefault(t => t.Expression != null);
            if(firstTransitionWithExpr == null)
                throw new Exception($"Invalid NFA - no expresion bound to first transition!");

            var ruleDef = (IRuleDef) firstTransitionWithExpr.Expression.Owner;

            c.BeginResolvingRuleDef(ruleDef);
            c.SetNfaForRuleDef(ruleDef, nfa);

            ResolveNfas(c, null, nfa);

            c.EndResolvingRuleDef(ruleDef);

            return nfa;
        }

        protected override IEnumerable<INfa> OnCreated(IEnumerable<INfa> nfas)
        {
            var c = new ResolveRulesContext();

            //First add the nfas to the context
            foreach (var nfa in nfas)
            {
                var firstTransitionWithExpr = nfa.Start.Transitions.FirstOrDefault(t => t.Expression != null);
                if (firstTransitionWithExpr == null)
                    throw new Exception($"Invalid NFA - no expresion bound to first transition!");

                var ruleDef = (IRuleDef)firstTransitionWithExpr.Expression.Owner;
                c.SetNfaForRuleDef(ruleDef, nfa);
            }

            foreach (var nfa in nfas)
            {
                var firstTransitionWithExpr = nfa.Start.Transitions.FirstOrDefault(t => t.Expression != null);
                if (firstTransitionWithExpr == null)
                    throw new Exception($"Invalid NFA - no expresion bound to first transition!");

                var ruleDef = (IRuleDef)firstTransitionWithExpr.Expression.Owner;
                ResolveNfas(c, ruleDef, nfa);
            }

            return nfas;
        }

        private void ResolveNfas(ResolveRulesContext c, IRuleDef ruleDef, INfa ruleNfa)
        {
            //We must resolve for the new nfa, since we just built it
            c.BeginResolvingRuleDef(ruleDef);

            ResolveNfas(c, ruleNfa.Start);

            c.EndResolvingRuleDef(ruleDef);
        }

        private void ResolveNfas(ResolveRulesContext c, INfaState state)
        {
            /*if (state.Id == 0)
                state.Id = _nextStateId++;*/

            var transitions = state.Transitions.ToList();
            foreach (var transition in transitions)
            {
                var toState = transition.ToState;
                /*if (toState.Id == 0)
                    toState.Id = _nextStateId++;*/

                var predictionTransition = transition as IEarleyNfaRulePredictionTransition;
                if (predictionTransition != null)
                {
                    //Don't enter predicted transitions... those only happen after a join... and would cause recursion.
                    continue;
                }

                var completionTransition = transition as IEarleyNfaRuleCompletionTransition;
                if (completionTransition != null)
                {
                    //Don't enter completion transitions... those only happen after a join... and would cause recursion.
                    continue;
                }

                var resolveRuleTransition = transition as IEarleyNfaResolveRuleTransition;
                if (resolveRuleTransition != null)
                {
                    var ruleDef = resolveRuleTransition.RuleDef;
                    //We are expected to resolve the rule - means builing the NFA for the rule and join it into the current nfa
                    var ruleNfa = c.GetNfaForRuleDef(ruleDef);
                    var isNew = false;
                    if (ruleNfa == null)
                    {
                        //Build a new one
                        ruleNfa = BuildForRuleRefExpression(ruleDef.Expression);
                        //Make sure to set it so we can reuse it during recursion
                        c.SetNfaForRuleDef(ruleDef, ruleNfa);
                        isNew = true;
                    }
                    
                    //Join the rule nfa!
                    JoinResolvedRuleNfa(state, resolveRuleTransition, ruleNfa);

                    if (isNew)
                    {
                        ResolveNfas(c, ruleDef, ruleNfa);
                    }
                }

                //Now proceed with building for the to state
                
                ResolveNfas(c, toState);
            }
        }

        private void JoinResolvedRuleNfa(INfaState fromState, IEarleyNfaResolveRuleTransition resolveRuleTransition, INfa ruleNfa)
        {
            //We will join in the rule nfa the nfa represented by the fromState and rule transition
            //The rule transition will basically be replaced by the new rule nfa
            var ruleDef = resolveRuleTransition.RuleDef;
            var ruleExpr = ruleDef.Expression;

            //Remove the transition from the current state
            fromState.RemoveTransition(resolveRuleTransition);

            //Join the fromState to the start of the rule nfa
            var fromStateEndToRuleStart = CreateRulePredictionTransition(fromState, ruleNfa.Start, ruleExpr, ruleDef);
            AssociateTransitionWithExpression(fromStateEndToRuleStart, ruleExpr);
            fromState.AddTransition(fromStateEndToRuleStart);

            //Now join the rule end to the "to" of the transition
            var fromRuleEndToTransitionTo = CreateRuleCompletionTransition(ruleNfa.End, resolveRuleTransition.ToState, ruleExpr, ruleDef);
            AssociateTransitionWithExpression(fromRuleEndToTransitionTo, ruleExpr);
            ruleNfa.End.AddTransition(fromRuleEndToTransitionTo);
        }

        private INfaTransition CreateRulePredictionTransition(INfaState fromState, INfaState toState, IExpr predictedRuleExpr, IRuleDef predictedRuleDef)
        {
            var t = new EarleyNfaRulePredictionTransition(predictedRuleDef);
            t.EnsureToState(toState);
            return t;
        }

        private INfaTransition CreateRuleCompletionTransition(INfaState end, INfaState toState, IExpr completedRuleExpr, IRuleDef completedRuleDef)
        {
            var t = new EarleyNfaRuleCompletionTransition(completedRuleDef);
            t.EnsureToState(toState);
            return t;
        }

        #region Build
        private INfa BuildForRuleRefExpression(IExpr ruleExpr)
        {
            //Build the nfa as usual
            return BuildForExpression(ruleExpr);
        }

        protected override INfa BuildForLeafExpression(IExpr expr)
        {
            var lexRefExpr = expr as ILexRefExpr;
            if (lexRefExpr != null)
            {
                //Lex refs can be built right away...
                return NfaForLex(lexRefExpr.LexDef, lexRefExpr);
            }

            var ruleRefExpr = expr as IRuleRefExpr;
            if (ruleRefExpr != null)
            {
                //Rule refs need to be built as a separate nfa and joined in later...
                //Add a "Rule Nfa node", which we will later build a separate nfa for and then be replaced with this nfa!
                return NfaForRule(ruleRefExpr.RuleDef, ruleRefExpr);
            }
            
            throw new Exception($"Unhandled leaf expression '{expr.ToString()}' of type '{expr.GetType().Name}'!");
        }
        #endregion

        #region Factory helpers
        public virtual INfa NfaForLex(ILexDef lexDef, IExpr expr)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            var lexDefTransition = CreateLexDefTransition(start, end, lexDef, expr);
            AssociateTransitionWithExpression(lexDefTransition, expr);
            start.AddTransition(lexDefTransition);

            var lexDefNfa = CreateLexDefNfa(start, end, lexDef, expr);
            AssociateNfaWithExpression(lexDefNfa, expr);

            return lexDefNfa;
        }

        public virtual INfa NfaForRule(IRuleDef ruleDef, IExpr expr)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            var ruleDefTransition = CreateRuleDefTransition(start, end, ruleDef, expr);
            AssociateTransitionWithExpression(ruleDefTransition, expr);
            start.AddTransition(ruleDefTransition);

            var ruleDefNfa = CreateRuleDefNfa(start, end, ruleDef, expr);
            AssociateNfaWithExpression(ruleDefNfa, expr);

            return ruleDefNfa;
        }
        #endregion

        #region Nfa factories
        protected override INfa CreateNfa(INfaState start, INfaState end, IExpr forExpression)
        {
            return new EarleyNfa(start, end);
        }

        protected virtual INfa CreateLexDefNfa(INfaState start, INfaState end, ILexDef lexDef, IExpr expr)
        {
            return new EarleyNfa(start, end);
        }

        protected virtual INfa CreateRuleDefNfa(INfaState start, INfaState end, IRuleDef ruleDef, IExpr expr)
        {
            return new EarleyNfa(start, end);
        }

        protected virtual INfaTransition CreateLexDefTransition(INfaState fromState, INfaState toState, ILexDef lexDef, IExpr expr)
        {
            var t = new EarleyNfaLexTransition(lexDef);
            t.EnsureToState(toState);
            return t;
        }
        
        protected virtual INfaTransition CreateRuleDefTransition(INfaState fromState, INfaState toState, IRuleDef ruleDef, IExpr expr)
        {
            var t = new EarleyNfaResolveRuleTransition(ruleDef);
            t.EnsureToState(toState);
            return t;
        }

        protected override INfaTransition CreateNullTransition(INfaState fromState, INfaState toState, IExpr forExpression)
        {
            var t = new EarleyNfaNullTransition();
            t.EnsureToState(toState);
            return t;
        }
        #endregion
    }
}
