using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Nfa;

namespace RapidPliant.Parsing.Automata.Nfa
{
    public class EarleyNfaAutomata : NfaAutomata<EarleyNfaAutomata>
    {
        protected override INfaBuilder CreateBuilder()
        {
            return new EarleyNfaBuilder();
        }
    }
}
