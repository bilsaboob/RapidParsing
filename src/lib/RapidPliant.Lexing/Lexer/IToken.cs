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

    public class TokenType<T> : TokenType
    {
        public TokenType(int id, string name, TokenCategory category = null)
            : base(id, name, category)
        {
        }

        public new T Ignore()
        {
            return (T)(object)base.Ignore();
        }
    }

    public class TokenType : IGrammarElement
    {
        public TokenType(int id, string name, TokenCategory category = null)
        {
            Id = id;
            Name = name;
            Category = category ?? TokenCategory.DEFAULT;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }

        public TokenCategory Category { get; private set; }

        public bool Ignored { get; private set; }

        public TokenType Ignore()
        {
            Ignored = true;
            return this;
        }

        public override string ToString()
        {
            return $"{Id}:{Name}";
        }
    }
    
    public class TokenCategory
    {
        public static readonly TokenCategory DEFAULT = new TokenCategory("default");

        public TokenCategory(string name, bool ignore = false)
        {
            Name = name;
            Ignored = ignore;
        }

        public string Name { get; private set; }
        public bool Ignored { get; private set; }

        public TokenCategory Ignore()
        {
            Ignored = true;
            return this;
        }
    }
}