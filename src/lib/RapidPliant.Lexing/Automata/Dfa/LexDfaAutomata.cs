using RapidPliant.Automata.Dfa;

namespace RapidPliant.Lexing.Automata.Dfa
{
    public class LexDfaAutomata : DfaAutomata<LexDfaAutomata>
    {
        protected override IDfaBuilder CreateBuilder()
        {
            return new LexDfaBuilder();
        }
    }
}
