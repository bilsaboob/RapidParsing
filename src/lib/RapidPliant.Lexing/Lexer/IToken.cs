namespace RapidPliant.Lexing.Lexer
{
    public interface IToken
    {
        int Index { get; }
        int Line { get; }
        int Col { get; }
        int Length { get; }
    }
}