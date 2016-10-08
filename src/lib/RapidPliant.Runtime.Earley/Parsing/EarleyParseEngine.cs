using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Runtime.Earley.Automata.Earley;
using RapidPliant.Runtime.Earley.Grammar;

namespace RapidPliant.Runtime.Earley.Parsing
{
    public class EarleyParseEngine
    {
        private IEarleyGrammar Grammar { get; set; }

        private EarleyChart Chart { get; set; }
        
        private int Location { get; set; }

        private IEarleyDfaState StartState { get; set; }
        
        private EarleySet CurrentSet { get; set; }

        public EarleyParseEngine(IEarleyGrammar grammar)
        {
            Initialize(grammar);
        }

        private void Initialize(IEarleyGrammar grammar)
        {
            Grammar = grammar;
            Location = 0;
            CurrentSet = GetEarleySet(0);
            
            StartState = Grammar.EarleyDfa.StartState;
            
            EnqueueState(CurrentSet, new EarleyState(StartState, CurrentSet));
        }

        public bool Pulse(int atLocation, IToken token, LexemeProcessList nextLexemes)
        {
            //Get the set at the given location
            var setAtLocation = GetEarleySet(atLocation);

            //Scan at the given location!
            var scannedAny = Scan(setAtLocation, token, CurrentSet, nextLexemes);
            if (!scannedAny)
                return false;

            //Move to the next set!
            CurrentSet = CurrentSet.NextEnsured;
            Location = CurrentSet.Location;

            Reduce(CurrentSet, nextLexemes);
            return true;
        }
        
        private bool Scan(EarleySet set, IToken token, EarleySet nextSet, LexemeProcessList nextLexemes)
        {
            var scannedAny = false;

            var earleyStates = set.EarleyStates.All;

            IEarleyDfaTransition dfaTransition;
            for (var i = 0; i < earleyStates.Length; ++i)
            {
                var earleyState = earleyStates[i];
                var origin = earleyState.OriginSet;
                var dfaState = earleyState.DfaState;

                //Get transition by token
                dfaTransition = dfaState.TokenTransitions.Get(token);
                if(dfaTransition == null)
                    continue;

                //Push into the next set to be processed
                var nextEarleyState = new EarleyState(dfaTransition, origin);
                if (!nextSet.EnqueueStateIfNotExists(nextEarleyState))
                    continue;
                
                //Update the next lexmes!
                nextLexemes.CollectFrom(nextEarleyState);

                scannedAny = true;

                //TODO: Handle null transition too!...
            }

            return scannedAny;
        }

        private bool Reduce(EarleySet set, LexemeProcessList nextLexemes)
        {
            var reducedAny = false;

            var setLocation = set.Location;
            var earleyStates = set.EarleyStates.All;
            
            for (var i = 0; i < earleyStates.Length; ++i)
            {
                var earleyState = earleyStates[i];
                var originSet = earleyState.OriginSet;
                var originSetLocation = originSet.Location;
                
                if(setLocation == originSetLocation)
                    continue;

                var dfaState = earleyState.DfaState;

                if (ReduceForState(set, originSet, dfaState, nextLexemes))
                    reducedAny = true;
            }

            return reducedAny;
        }

        private bool ReduceForState(EarleySet set, EarleySet originSet, IEarleyDfaState dfaState, LexemeProcessList nextLexemes)
        {
            var reducedAny = false;
            var originEarleyStates = originSet.EarleyStates.All;

            var completions = dfaState.Completions;
            for (var i = 0; i < completions.Length; ++i)
            {
                var completion = completions[i];

                for (var j = 0; j < originEarleyStates.Length; ++j)
                {
                    var originEarleyState = originEarleyStates[j];
                    var originDfa = originEarleyState.DfaState;

                    var dfaTransition = originDfa.RuleTransitions.Get(completion.RuleDef);
                    if(dfaTransition == null)
                        continue;

                    //Completion, so we reference the "origin origin" set... one step back up the chain!
                    var originOriginEarleySet = originEarleyState.OriginSet;
                    var nextEarleyState = new EarleyState(dfaTransition, originOriginEarleySet);
                    if(!set.EnqueueStateIfNotExists(nextEarleyState))
                        continue;

                    //Update next lexemes for the next earley state
                    nextLexemes.CollectFrom(nextEarleyState);

                    reducedAny = true;

                    //TODO: Handle null transition too!...
                }
            }

            return reducedAny;
        }

        #region helpers
        private void EnqueueState(EarleySet set, EarleyState earleyState)
        {
            set.EnqueueStateIfNotExists(earleyState);
        }

        private EarleySet GetEarleySet(int location)
        {
            return Chart.GetEarleySet(location);
        }
        #endregion
    }

    public class EarleyChart
    {
        private EarleySetList Sets { get; set; }

        public EarleyChart()
        {
            Sets = new EarleySetList(this);
        }

        public EarleySet GetEarleySet(int location)
        {
            if (location > Sets.Count)
            {
                throw new Exception("Cannot jump over a location");
            }
            else if (location == Sets.Count)
            {
                return Sets.PushNewSet();
            }
            else
            {
                return Sets.Get(location);
            }
        }
    }

    public class EarleySet
    {
        private EarleyChart _chart;
        
        public EarleySet(EarleyChart chart, EarleySet prevSet, int location)
        {
            _chart = chart;

            Prev = prevSet;
            Location = location;

            EarleyStates = new EarleyStateList();
        }

        public EarleySet Prev { get; internal set; }
        public EarleySet Next { get; internal set; }
        public int Location { get; internal set; }

        public EarleySet NextEnsured
        {
            get
            {
                if (Next != null)
                    return Next;

                Next = _chart.GetEarleySet(Location + 1);

                return Next;
            }
        }

        public EarleyStateList EarleyStates { get; private set; }

        public bool EnqueueStateIfNotExists(EarleyState earleyState)
        {
            //Enqueue only if the earleyState is unique
            return EarleyStates.AddIfNotExists(earleyState);
        }
    }

    public class EarleySetList
    {
        private List<EarleySet> _list;

        private EarleyChart _chart;
        private EarleySet _lastSet;


        public EarleySetList(EarleyChart chart)
        {
            _chart = chart;

            _list = new List<EarleySet>();
        }
        
        public int Count { get; private set; }

        public EarleySet PushNewSet()
        {
            var set = new EarleySet(_chart, _lastSet, Count);

            if (_lastSet != null)
            {
                _lastSet.Next = set;
            }

            _lastSet = set;

            return set;
        }

        public EarleySet Get(int location)
        {
            return _list[location];
        }
    }

    public class EarleyStateList
    {
        public EarleyState[] All { get; private set; }

        public bool AddIfNotExists(EarleyState earleyState)
        {
            return false;
        }
    }
}
