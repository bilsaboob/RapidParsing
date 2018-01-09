namespace RapidPliant.Lexing.Lexer
{
    public interface IToken
    {
        int Index { get; }
        int Line { get; }
        int Col { get; }
        int Length { get; }

        object TokenType { get; }
    }

    public interface ITokenStream
    {
        object State { get; }
        void Init(object state);

        bool IsAtEnd { get; }

        IToken Token { get; }
        bool MoveNext();
    }
}