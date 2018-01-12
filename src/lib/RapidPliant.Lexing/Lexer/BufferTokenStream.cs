using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Lexing.Lexer
{
    public class BufferTokenStream : ITokenStream
    {
        private List<IToken> _tokens;
        private int _index;
        private IToken _token;

        public BufferTokenStream(List<IToken> tokens)
        {
            _tokens = tokens;
            _index = 0;
        }

        public IReadOnlyList<IToken> Tokens => _tokens;

        public IToken Token => _token;
        
        public bool IsAtEnd => _index >= _tokens.Count;

        public bool MoveNext()
        {
            if (_index < _tokens.Count)
            {
                _token = _tokens[_index];
                ++_index;
                return true;
            }

            return false;
        }

        #region State
        public object GetState()
        {
            return new State(_index, _index-1);
        }

        public void Reset(object state)
        {
            var s = (State)state;
            _index = s.Index;
            var tokenIndex = s.TokenIndex;
            if (tokenIndex >= 0)
            {
                _token = _tokens[tokenIndex];
            }
            else
            {
                _token = null;
            }
        }

        public void Reset()
        {
            _index = 0;
        }

        class State
        {
            public State(int index, int tokenIndex)
            {
                Index = index;
                TokenIndex = tokenIndex;
            }

            public int TokenIndex { get; set; }
            public int Index { get; set; }

            public State Clone()
            {
                return new State(Index, TokenIndex);
            }
        }
        #endregion
    }
}
