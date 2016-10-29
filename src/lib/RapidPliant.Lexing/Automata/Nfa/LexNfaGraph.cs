using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Lexing.Automata
{
    public partial class LexNfaAutomata
    {
        public class LexNfaGraph : NfaGraphBase
        {
            public LexNfaGraph(LexNfa nfa) : base(nfa) { }
        }

        public class LexNfaGraphBuildContext : NfaGraphBuildContextBase
        {
        }
    }
}
