namespace RapidPliant.Runtime.Earley.Lex.Util
{
    public class LexDfaStatesCollection
    {
        public bool TryGetOrAdd(LexDfaState dfaStateToAdd, out LexDfaState addedOrExistingState)
        {
            addedOrExistingState = null;
            return true;
        }
    }
}