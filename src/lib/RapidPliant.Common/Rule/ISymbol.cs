namespace RapidPliant.Common.Rule
{
    public interface ISymbol
    {
        int IndexForOwnerRule { get; }
        int IndexForOwnerSet { get; }
        int IndexForGlobal { get; }

        SymbolType SymbolType { get; set; }
    }

    public class SymbolType
    {
    }
}