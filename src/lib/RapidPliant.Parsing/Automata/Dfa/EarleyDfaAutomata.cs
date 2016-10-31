using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;

namespace RapidPliant.Parsing.Automata.Dfa
{
    public class EarleyDfaAutomata : DfaAutomata<EarleyDfaAutomata>
    {
        protected override IDfaBuilder CreateBuilder()
        {
            return new EarleyDfaBuilder();
        }
    }
}
