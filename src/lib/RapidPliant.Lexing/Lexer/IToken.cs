namespace RapidPliant.Lexing.Lexer
{
    public interface IToken
    {
        int Index { get; }
        int Line { get; }
        int Col { get; }
        int Length { get; }

        object TokenType { get; }

        bool IsBadToken { get; }
    }

    public interface ITokenStream
    {
        object GetState();
        void Reset(object state);

        bool IsAtEnd { get; }

        IToken Token { get; }
        bool MoveNext();
    }

    public interface IGrammarElement
    {
        int Id { get; }
        string Name { get; }
    }

    public class TokenType : IGrammarElement
    {
        public TokenType(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }

        public override string ToString()
        {
            return $"{Id}:{Name}";
        }
    }
}