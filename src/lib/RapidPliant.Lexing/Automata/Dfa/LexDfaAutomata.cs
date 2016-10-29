using RapidPliant.Automata.Dfa;

namespace RapidPliant.Lexing.Automata.Dfa
{
    public class LexDfaAutomata : DfaAutomata<LexDfaAutomata>
    {
        public LexDfaAutomata()
            : base(new LexDfaBuilder())
        {
        }
    }
}
