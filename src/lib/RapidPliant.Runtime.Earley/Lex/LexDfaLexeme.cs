using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Runtime.Earley.Lex
{
    public class LexDfaLexeme
    {
        private LexDfa Dfa { get; set; }

        private LexDfaState CurrentState { get; set; }

        public LexDfaLexeme(LexDfa lexDfa)
        {
            Dfa = lexDfa;
            CurrentState = Dfa.StartState;
        }

        public bool CanAcceptAny
        {
            get
            {
                //CurrentState.Completions.Count > 0;
                return false;
            }
        }

        public void ScanNext(char c)
        {
            var t = CurrentState.GetTransitionForChar(c);
            if (t == null)
            {
                //No transition!
                return;
            }

            var toState = t.ToState;
            CurrentState = toState;
        }
    }
}
