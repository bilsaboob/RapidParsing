using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Text;

namespace RapidPliant.Lexing.Lexer
{
    public struct Position
    {
        public int Index { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }
    }

    public class Lexer
    {
        protected ITokenizer _tokenizer;

        private int _index;
        private int _col;
        private int _line;

        private int _i;
        private char _ch;

        private IBuffer _buffer;
        private int _bufferLen;

        private bool _canContinue;

        private LexContext _lexContext;
        
        public Lexer(ITokenizer tokenizer)
        {
            _tokenizer = tokenizer;
            _lexContext = new LexContext();
        }

        public bool CanContinue => !IsAtEnd && _canContinue;
        public bool IsAtEnd => _index >= _bufferLen;

        public void Init(IBuffer buffer, Position startPosition = default(Position))
        {
            _buffer = buffer;
            _bufferLen = _buffer.Length;

            // init the tokenizer
            _ch = '\0';
            _i = _ch;

            _index = startPosition.Index;
            _line = startPosition.Line;
            _col = startPosition.Col;

            _tokenizer.Init(_index);
            _canContinue = true;
        }

        public IToken Lex()
        {
            if (!CanContinue) return null;
            
            ISpellingCapture capture = null;
            ISpellingCapture lastCapture = null;

            // Initialize a first advance
            if (_i == 0)
            {
                Advance(false);
                if (!CanContinue) return null;
            }
            
            // skip any ignores before the token
            SkipIgnores();

            if (!CanContinue) return null;

            // reset the tokenizer for the next token 
            _tokenizer.Init(_index);

            // now mark the actual token start
            var startIndex = _index;
            var startLine = _line;
            var startCol = _col;

            // lex the token
            while (CanContinue)
            {
                _lexContext.CharToLex = _ch;
                _tokenizer.Tokenize(_lexContext);

                capture = _lexContext.Capture;
                if (capture != null)
                    lastCapture = capture;

                if(!_tokenizer.CanContinue)
                    break;

                Advance();
            }

            // check if we have a capture => we have a new token
            if (lastCapture != null)
            {
                // we have a token
                if (startIndex > 0)
                    startIndex = startIndex - 1;

                var token = new Token(startIndex, startLine, startCol, _index - startIndex - 1);

                // skip any ignores after the token
                if (CanContinue)
                {
                    SkipIgnores();
                }
                
                return token;
            }
            else
            {
                // no available token, lex error!
                _canContinue = false;
            }
            
            return null;
        }
        
        protected virtual void SkipIgnores()
        {
            char ch;
            //skip any ignore tokens - per default it's any whitespace
            while (CanContinue)
            {
                ch = _ch;

                if (!char.IsWhiteSpace(ch))
                    break;

                if (ch == '\n')
                {
                    ++_line;
                    _col = 0;
                }

                Advance();
            }
        }

        private void Advance(bool updatePosition = true)
        {
            //advance to next character
            _i = _buffer[_index];
            _ch = (char)_i;

            if (updatePosition)
            {
                ++_index;
                if (_ch == '\n')
                {
                    ++_line;
                    _col = 0;
                }
                else
                {
                    ++_col;
                }
            }
        }
    }

    class Token : IToken
    {
        public Token(int index, int line, int col, int length)
        {
            Index = index;
            Line = line;
            Col = col;
            Length = length;
        }

        public int Index { get; }
        public int Line { get; }
        public int Col { get; }
        public int Length { get; }
    }
}
