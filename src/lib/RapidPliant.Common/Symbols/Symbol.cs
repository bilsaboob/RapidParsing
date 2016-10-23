namespace RapidPliant.Common.Symbols
{
    public enum SymbolType
    {
        Terminal,
        Lex,
        Parse
    }

    public interface ISymbol
    {
        SymbolType SymbolType { get; }
    }

    public abstract class Symbol : ISymbol
    {
        public Symbol(SymbolType symbolType)
        {
            SymbolType = symbolType;
        }

        public SymbolType SymbolType { get; protected set; }
    }
}