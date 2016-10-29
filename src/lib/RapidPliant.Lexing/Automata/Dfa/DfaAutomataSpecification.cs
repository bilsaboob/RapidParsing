using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Lexing.Automata.Dfa
{
    public class DfaAutomataSpecification<TAutomata, TTransitionValue, TDfaBuilder, TDfaStateBuilder, TDfaBuildContext>
        where TAutomata : new()
        where TDfaBuilder : DfaBuilder<TTransitionValue, TDfaStateBuilder, TDfaBuildContext>
        where TDfaStateBuilder : DfaStateBuilder<TTransitionValue, TDfaBuildContext>
        where TDfaBuildContext : DfaBuildContext
    {
        private static TAutomata _instance;
        public static TAutomata Instance
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

        public abstract class DfaBuilderBase : DfaBuilder<TTransitionValue, TDfaStateBuilder, TDfaBuildContext>
        {
        }

        public abstract class DfaStateBuilderBase : DfaStateBuilder<TTransitionValue, TDfaBuildContext>
        {
            public DfaStateBuilderBase(NfaClosure closure) : base(closure)
            {
            }
        }

        public abstract class DfaBuildContextBase : DfaBuildContext
        {
            public DfaBuildContextBase(INfa nfa) : base(nfa)
            {
            }
        }
    }
}
