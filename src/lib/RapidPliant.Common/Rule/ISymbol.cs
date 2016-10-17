namespace RapidPliant.Common.Rule
{
    public interface ISymbol
    {
        SymbolType SymbolType { get; }
    }

    public abstract class Symbol : ISymbol
    {
        public Symbol()
        {
        }

        public SymbolType SymbolType { get; protected set; }
    }

    public class SymbolType
    {
    }
}