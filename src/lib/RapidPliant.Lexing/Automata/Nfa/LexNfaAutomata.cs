using RapidPliant.Automata.Nfa;

namespace RapidPliant.Lexing.Automata.Nfa
{
    public class LexNfaAutomata : NfaAutomata<LexNfaAutomata>
    {
        public LexNfaAutomata()
            : base(new LexNfaBuilder())
        {
        }
    }
}
