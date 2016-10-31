using RapidPliant.Automata.Nfa;

namespace RapidPliant.Lexing.Automata.Nfa
{
    public class LexNfaAutomata : NfaAutomata<LexNfaAutomata>
    {
        protected override INfaBuilder CreateBuilder()
        {
            return new LexNfaBuilder();
        }
    }
}
