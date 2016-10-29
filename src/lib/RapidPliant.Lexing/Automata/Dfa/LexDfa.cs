using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Automata.Nfa;
using RapidPliant.Common.Symbols;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Automata.Dfa
{
    public interface ILexDfaTransition : IDfaTransition
    {
        Interval Interval { get; }
        IEnumerable<ITerminal> Terminals { get; }
    }

    public class LexDfaTransition : DfaTransition, ILexDfaTransition
    {
        public LexDfaTransition(Interval transitionValue, IEnumerable<INfaTransition> nfaTransitons, IEnumerable<INfaTransition> nfaFinalTransitions, DfaState toState)
            : base(transitionValue, nfaTransitons, nfaFinalTransitions, toState)
        {
            Interval = transitionValue;
        }

        public Interval Interval { get; protected set; }
    }
}
