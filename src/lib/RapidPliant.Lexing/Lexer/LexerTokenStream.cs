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
        private List<IToken> _cachedTokens;
        private int _cachedCount;

        public LexerTokenStream(Lexer lexer)
        {
            _lexer = lexer;
            _state = new StreamState();
            _cachedTokens = new List<IToken>();
        }

        public object State => _state.Clone();

        public void Init(object state)
        {
            _state = (StreamState) state;
        }

        public bool IsAtEnd => _lexer.IsAtEnd;

        public IToken Token { get; private set; }

        public bool MoveNext()
        {
            if (_state.Index >= _cachedCount)
            {
                if (!_lexer.CanContinue)
                    return false;
                
                Token = _lexer.Lex();
                ++_state.Index;

                if (Token == null)
                    return false;

                // cache the token
                _cachedTokens.Add(Token);
                ++_cachedCount;
            }
            else
            {
                // use cached token
                Token = _cachedTokens[_state.Index++];
            }
            
            return true;
        }

        class StreamState
        {
            // the token position
            public int Index { get; set; }

            public StreamState Clone()
            {
                return new StreamState() {
                    Index = Index
                };
            }
        }
    }
}
