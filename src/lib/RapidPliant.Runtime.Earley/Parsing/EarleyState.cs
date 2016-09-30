using RapidPliant.Runtime.Earley.Automata.Earley;

namespace RapidPliant.Runtime.Earley.Parsing
{
    public class EarleyState
    {
        public EarleyState(IEarleyDfaState dfaState, EarleySet originSet)
        {
            DfaState = dfaState;
            OriginSet = originSet;
            OriginLocation = originSet.Location;
        }

        public EarleyState(IEarleyDfaTransition fromTransition, EarleySet originSet)
        {
            FromTransition = fromTransition;
            DfaState = FromTransition.ToState;
            OriginSet = originSet;
            OriginLocation = OriginSet.Location;
        }

        public int OriginLocation { get; private set; }

        public EarleySet OriginSet { get; private set; }

        public IEarleyDfaState DfaState { get; private set; }

        public IEarleyDfaTransition FromTransition { get; set; }
    }
}