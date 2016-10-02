using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar;
using RapidPliant.Runtime.Earley.Grammar;

namespace RapidPliant.Runtime.Earley.Automata.Earley
{
    public interface IEarleyNfa
    {
        IEarleyNfaState Start { get; }
        IEarleyNfaState End { get; }
    }

    public interface IEarleyNfaState
    {
    }

    public class EarleyNfa : IEarleyNfa
    {
        public EarleyNfa()
            : this(new EarleyNfaState(), new EarleyNfaState())
        {
        }

        public EarleyNfa(EarleyNfaState startState, EarleyNfaState endState)
        {
            Start = startState;
            End = endState;
        }

        public EarleyNfaState Start { get; set; }
        public EarleyNfaState End { get; set; }

        public EarleyNfa Optional()
        {
            var newStart = new EarleyNfaState();
            var newEnd = new EarleyNfaState();

            newStart.AddNullTransitionTo(Start);
            newStart.AddNullTransitionTo(newEnd);
            
            return new EarleyNfa(newStart, newEnd);
        }

        public EarleyNfa UnionWith(EarleyNfa otherNfa)
        {
            var newStart = new EarleyNfaState();
            var newEnd = new EarleyNfaState();

            newStart.AddNullTransitionTo(Start);
            newStart.AddNullTransitionTo(otherNfa.Start);

            End.AddNullTransitionTo(newEnd);
            otherNfa.End.AddNullTransitionTo(newEnd);

            return new EarleyNfa(newStart, newEnd);
        }

        public EarleyNfa ConcatWith(EarleyNfa otherNfa)
        {
            End.AddNullTransitionTo(otherNfa.Start);
            return this;
        }

        IEarleyNfaState IEarleyNfa.Start
        {
            get { return Start; }
        }

        IEarleyNfaState IEarleyNfa.End
        {
            get { return End; }
        }
    }
    
    public class EarleyNfaState : IEarleyNfaState
    {
        private List<EarleyNfaTransition> _transitions;

        public EarleyNfaState()
        {
            _transitions = new List<EarleyNfaTransition>();
        }
        
        public void AddLexTransitionTo(ILexDef lexDef, EarleyNfaState toState)
        {
            //Add transition to the specified state for the given lexExpr
            _transitions.Add(new EarleyNfaLexTransition(lexDef, this, toState));
        }

        public void AddNullTransitionTo(EarleyNfaState toState)
        {
            //Add transition to the specified state for the given lexExpr
            _transitions.Add(new EarleyNfaNullTransition(this, toState));
        }

        public void AddRuleTransitionTo(IRuleDef ruleDef, EarleyNfaState toState)
        {
            //Add transition by rule
            _transitions.Add(new EarleyNfaRuleTransition(ruleDef, this, toState));
        }
    }

    public interface IEarleyNfaTransition
    {
    }

    public abstract class EarleyNfaTransition : IEarleyNfaTransition
    {
        public EarleyNfaTransition(EarleyNfaState fromState, EarleyNfaState toState)
        {
            FromState = fromState;
            ToState = toState;
        }

        public EarleyNfaState FromState { get; protected set; }
        public EarleyNfaState ToState { get; protected set; }
    }

    public class EarleyNfaLexTransition : EarleyNfaTransition
    {
        public EarleyNfaLexTransition(ILexDef lexDef, EarleyNfaState fromState, EarleyNfaState toState)
            : base(fromState, toState)
        {
            LexDef = lexDef;
        }

        public ILexDef LexDef { get; private set; }
    }

    public class EarleyNfaNullTransition : EarleyNfaTransition
    {
        public EarleyNfaNullTransition(EarleyNfaState fromState, EarleyNfaState toState)
            : base(fromState, toState)
        {
        }
    }

    public class EarleyNfaRuleTransition : EarleyNfaTransition
    {
        public EarleyNfaRuleTransition(IRuleDef ruleDef, EarleyNfaState fromState, EarleyNfaState toState)
            : base(fromState, toState)
        {
            RuleDef = ruleDef;
        }

        public IRuleDef RuleDef { get; private set; }
    }
}
