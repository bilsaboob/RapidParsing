using RapidPliant.Automata.Nfa;

namespace RapidPliant.Automata.Dfa
{
    public abstract class DfaAutomata<TAutomata>
        where TAutomata : DfaAutomata<TAutomata>, new()
    {
        #region static factory helpers
        private static TAutomata _instance;
        private static TAutomata Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TAutomata();
                }

                return _instance;
            }
        }

        public static IDfaState BuildDfa(INfaGraph nfaGraph)
        {
            return Instance.CreateNfaForNfaGraph(nfaGraph);
        }
        
        #endregion

        public DfaAutomata(IDfaBuilder dfaBuilder)
        {
            DfaBuilder = dfaBuilder;
        }

        protected IDfaBuilder DfaBuilder { get; set; }

        public virtual IDfaState CreateNfaForNfaGraph(INfaGraph nfaGraph)
        {
            return DfaBuilder.Create(nfaGraph);
        }
    }
}
