using RapidPliant.Common.Rule;

namespace RapidPliant.Lexing.Dfa
{
    public class LexDfaProductionState
    {
        public LexDfaProductionState(LexDfaProductionPath forPath)
        {
            Production = forPath.Production;
            SymbolIndex = forPath.SymbolIndex;
            Symbol = forPath.Symbol;
            IsEnd = forPath.IsAtEnd;
        }

        public IProduction Production { get; private set; }
        public int SymbolIndex { get; private set; }
        public ISymbol Symbol { get; private set; }
        public bool IsEnd { get; private set; }
    }
}