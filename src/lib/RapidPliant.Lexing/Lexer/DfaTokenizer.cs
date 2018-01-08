using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Common.Expression;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Lexer.Recognition;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Lexer
{
    public class DfaTokenizer : ITokenizer, IDisposable
    {
        private CharBuffer _spelling;
        
        private bool _canContinue;

        protected ILexRecognizer _recognizer;
        
        public DfaTokenizer(ILexRecognizer recognizer)
        {
            _recognizer = recognizer;
            _spelling = new CharBuffer(new StringBuilder());

            Reset();
        }

        public bool CanContinue { get { return _canContinue; } }
        
        public void Init(int index)
        {
            Reset();
        }

        private void Reset()
        {
            _recognizer.Reset();

            _spelling.Clear();

            _canContinue = true;
        }

        public bool Tokenize(ILexContext context)
        {
            var ch = context.CharToLex;

            //Let the recognizer check if we can move forward for the specified char
            var r = _recognizer.Recognize(ch);
            if (r == null)
            {
                //No more valid recognition!
                _canContinue = false;
                return false;
            }

            //Append the spelling
            _spelling.Append(ch);

            //Only pick the last completion
            var completions = r.Completions;
            if (completions != null && completions.Count > 0)
            {
                context.Capture = new BufferedSpellingCapture(_spelling, 0, _spelling.Length, r, completions[completions.Count-1]);
            }

            return true;
        }

        public void Dispose()
        {
            if (_spelling != null)
            {
                _spelling.Dispose();
            }
        }
    }
    
    public class CharBuffer
    {
        private StringBuilder _sb;
        private int _length;
        private string _str;

        public CharBuffer(StringBuilder sb)
        {
            _sb = sb;
        }

        public int Length { get { return _length; } }

        public void Append(char ch)
        {
            _sb.Append(ch);
            _length = _sb.Length;
            //_length += 1;
            _str = null;
        }

        public string GetText(int startIndex, int endIndex)
        {
            var count = endIndex - startIndex;
            //var text = new char[count];
            var text = new char[count];
            _sb.CopyTo(startIndex, text, 0, count);
            return new string(text);
        }

        public string GetAllText()
        {
            if(_str == null)
                _str = _sb.ToString();
            return _str;
        }

        public void Clear()
        {
            if (_sb != null)
            {
                _sb.Clear();
                _str = null;
            }
        }

        public void Dispose()
        {
            if (_sb != null)
            {
                _sb.ClearAndFree();
                _sb = null;
                _str = null;
                _length = 0;
            }
        }
    }
    
    public class BufferedSpellingCapture : RangeSpellingCapture
    {
        private string _spelling;
        private CharBuffer _charBuffer;

        public BufferedSpellingCapture(CharBuffer charBuffer, int startIndex, int endIndex, IDfaRecognition recognition, IRecognizerCompletion completion)
            : base(startIndex, endIndex, recognition, completion)
        {
            _charBuffer = charBuffer;
        }

        public override string Spelling
        {
            get
            {
                if (_spelling == null)
                {
                    _spelling = _charBuffer.GetText(_startIndex, _endIndex);
                }
                return _spelling;
            }
        }
    }
}

