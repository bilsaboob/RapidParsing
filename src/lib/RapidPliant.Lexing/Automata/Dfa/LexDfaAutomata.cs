using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Lexing.Automata.Dfa;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata
{
    public abstract class LexDfaAutomataSpecification : DfaAutomataSpecification<
        LexDfaAutomata,
        Interval,
        LexDfaAutomata.LexDfaBuilder,
        LexDfaAutomata.LexDfaStateBuilder,
        LexDfaAutomata.LexDfaBuildContext
        >
    {
    }

    public partial class LexDfaAutomata : LexDfaAutomataSpecification
    {
        #region static helpers
        public static DfaState BuildDfa(INfa nfa)
        {
            return BuildDfa(nfa.ToNfaGraph());
        }

        public static DfaState BuildDfa(INfaGraph nfaGraph)
        {
            return Instance.CreateForNfa(nfaGraph);
        }
        #endregion

        public LexDfaAutomata()
        {
            DfaBuilder = new LexDfaBuilder();
        }

        public LexDfaBuilder DfaBuilder { get; private set; }

        public DfaState CreateForNfa(INfaGraph nfaGraph)
        {
            return DfaBuilder.Create(nfaGraph);
        }
    }
    
    public partial class LexDfaAutomata : LexDfaAutomataSpecification
    {
        public class LexDfaBuildContext : DfaBuildContextBase
        {
            public LexDfaBuildContext(INfa nfa) : base(nfa) {}
        }

        public class LexDfaBuilder : DfaBuilderBase
        {
            protected override LexDfaBuildContext CreateBuildContext(INfa nfa)
            {
                return new LexDfaBuildContext(nfa);
            }

            protected override LexDfaStateBuilder CreateStateBuilder(NfaClosure closure)
            {
                return new LexDfaStateBuilder(closure);
            }
        }

        public class LexDfaStateBuilder : DfaStateBuilderBase
        {
            public LexDfaStateBuilder(NfaClosure closure)
                : base(closure)
            {
            }

            protected override DfaTransition CreateDfaTransition(Interval transitionValue, IEnumerable<INfaTransition> nfaTransitions, HashSet<INfaTransition> finalNfaTransitions, DfaState toDfaState)
            {
                var dfaTransition = new DfaTransition(transitionValue, nfaTransitions, finalNfaTransitions, toDfaState);

                //Collect terminals for all nfa transitions
                CollectTerminals(dfaTransition, nfaTransitions);

                //Collect completions for final nfa transitions
                CollectCompletions(dfaTransition, finalNfaTransitions);

                return dfaTransition;
            }

            protected override void BuildFornNfaTransition(INfaTransition transition)
            {
                //We only handle terminal transitions for building the DFA!

                var terminalTransition = transition as ILexNfaTerminalTransition;
                if (terminalTransition == null)
                    return;

                //Here we must take into account the different char intervals a terminal can have!
                if (terminalTransition.FromState == null)
                {
                    throw new Exception("FromState is required!");
                }

                //Instead of storing transitions per terminal - we must store the transitions per character range part of the terminal
                var terminal = terminalTransition.Terminal;
                if(terminal == null)
                    return;

                var intervals = terminal.GetIntervals();
                foreach (var interval in intervals)
                {
                    var transitionEntry = ClosureTransitions.AddTransition(interval, terminalTransition);

                    //Set the terminal too!
                    transitionEntry.AddTerminal(terminal);
                }
            }

            protected override IEnumerable<INfaTransitionsByValue<Interval>> GetNfaTransitionsByValue()
            {
                //Get ther transitions, there is a risk that these may be overlapping!
                var transitions = ClosureTransitions.GetTransitionsByTerminalIntervals();

                //We need to split the terminal transitions into unique non overlapping intervals
                var nonOverlappingIntervalTransitions = new NonOverlappingIntervalSet<INfaTransition>();
                foreach (var transitionsByTerminalIntervals in transitions)
                {
                    var interval = transitionsByTerminalIntervals.TransitionValue;
                    var nfaTransitions = transitionsByTerminalIntervals.Transitions;

                    //Add the interval, which will split into non overlapping intervals, but keep the association of the nfa transitions for each of the splits
                    nonOverlappingIntervalTransitions.AddInterval(interval, nfaTransitions);
                }

                //Return the non overlapping ones!
                foreach (var transitionsByTerminalIntervals in nonOverlappingIntervalTransitions)
                {
                    var transitionInterval = transitionsByTerminalIntervals.Interval;
                    var nfaTransitions = transitionsByTerminalIntervals.AssociatedItems;

                    yield return new NonOverlappingNfaTransitionsByValue(transitionInterval, nfaTransitions);
                }
            }
            
            class NonOverlappingNfaTransitionsByValue : INfaTransitionsByValue<Interval>
            {
                public NonOverlappingNfaTransitionsByValue(Interval interval, IEnumerable<INfaTransition> nfaTransitions)
                {
                    TransitionValue = interval;
                    Transitions = nfaTransitions;
                }

                public Interval TransitionValue { get; }
                public IEnumerable<INfaTransition> Transitions { get; }
            }

            private void CollectTerminals(DfaTransition dfaTransition, IEnumerable<INfaTransition> nfaTransitions)
            {
                if (nfaTransitions == null)
                    return;

                //Collect the terminals for the dfa transition
                foreach (var nfaTransition in nfaTransitions)
                {                    
                    var terminalTransition = nfaTransition as ILexNfaTerminalTransition;
                    if (terminalTransition != null)
                    {
                        dfaTransition.AddTerminal(terminalTransition.Terminal);
                    }
                }
            }

            private void CollectCompletions(DfaTransition dfaTransition, IEnumerable<INfaTransition> nfaTransitions)
            {
                if(nfaTransitions == null)
                    return;

                var completionsByExpression = ReusableDictionary<IExpr, DfaCompletion>.GetAndClear();

                //Collect the grouped dfa completions for the dfa transition - group by "Rule" / "Expresion"
                foreach (var nfaTransition in nfaTransitions)
                {
                    var expr = nfaTransition.Expression;
                    if (expr == null)
                        continue;

                    DfaCompletion completion;
                    if (!completionsByExpression.TryGetValue(expr, out completion))
                    {
                        completion = new DfaCompletion(dfaTransition, expr);
                        dfaTransition.AddCompletion(completion);
                        completionsByExpression[expr] = completion;
                    }

                    completion.AddNfaTransition(nfaTransition);
                }

                completionsByExpression.ClearAndFree();
            }
        }
    }
}
