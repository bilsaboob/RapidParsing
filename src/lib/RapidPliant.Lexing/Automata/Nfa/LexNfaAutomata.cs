using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;

namespace RapidPliant.Lexing.Automata
{
    public class LexNfaAutomata : NfaAutomata<LexNfaAutomata>
    {
        public LexNfaAutomata()
            : base(new LexNfaBuilder())
        {
        }
    }
}
