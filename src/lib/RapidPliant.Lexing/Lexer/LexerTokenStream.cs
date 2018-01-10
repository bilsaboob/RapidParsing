using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Text;

namespace RapidPliant.Lexing.Lexer
{
    public class LexerTokenStream : ITokenStream
    {
        private Lexer _lexer;
        private StreamState _state;

        public LexerTokenStream(Lexer lexer)
        {
            _lexer = lexer;
            _state = new StreamState();
        }

        public object GetState()
        {
            return new StreamState(Token, _lexer.GetState());
        }

        public void Reset(object state)
        {
            _state = (StreamState) state;
            _lexer.Reset(_state.LexerState);
            Token = _state.Token;

            // reset the state and move next
            //MoveNext();
        }

        public bool IsAtEnd => _lexer.IsAtEnd;

        public IToken Token { get; set; }

        public bool MoveNext()
        {
            // never continue if at end
            if (IsAtEnd) return false;
                
            // Lex the token
            Token = _lexer.Lex();
            if (Token == null)
            {
                // we failed reading a token, so we have a "bad token"... let's skip character one by one until we get to the next available good token
                while (!IsAtEnd)
                {
                    var badToken = _lexer.SkipUntilPossibleNextStart();
                    if (badToken != null)
                    {
                        Token = badToken;
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        class StreamState
        {
            public StreamState()
            {
            }

            public StreamState(IToken token, object lexerState)
            {
                Token = token;
                LexerState = lexerState;
            }

            public IToken Token { get; set; }

            public object LexerState { get; set; }
            
            public StreamState Clone()
            {
                return new StreamState() {
                    Token = Token,
                    LexerState = LexerState
                };
            }
        }
    }

    public static class TokenStreamExtensions
    {
        public static List<IToken> ReadAllTokens(this LexerTokenStream stream)
        {
            var tokens = new List<IToken>();

            while (stream.MoveNext())
            {
                tokens.Add(stream.Token);
            }

            return tokens;
        }
    }
}
