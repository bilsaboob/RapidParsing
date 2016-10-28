using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Lexer
{
    public class DfaLexer : ILexer, IDisposable
    {
        private CharBuffer _spelling;
        
        private bool _canContinue;

        protected IDfaRecognizer<char> _recognizer;
        
        public DfaLexer(IDfaRecognizer<char> recognizer)
        {
            _recognizer = recognizer;
            
            Reset();
        }

        public bool CanContinue { get { return _canContinue; } }
        
        public void Init()
        {
            Reset();
        }

        private void Reset()
        {
            _recognizer.Reset();

            _spelling = new CharBuffer(ReusableStringBuilder.GetAndClear());
            
            _canContinue = true;
        }

        public bool Lex(ILexContext context)
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

            //Check for any completions and add them to the lex context if any!
            var completions = r.Completions;
            if (completions != null)
            {
                foreach (var completion in completions)
                {
                    var capture = new SpellingCapture(_spelling, 0, _spelling.Length, r, completion);
                    context.AddCapture(capture);
                }
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

        public CharBuffer(StringBuilder sb)
        {
            _sb = sb;
        }

        public int Length { get { return _length; } }

        public void Append(char ch)
        {
            _sb.Append(ch);
            _length = _sb.Length;
        }

        public string GetText(int startIndex, int endIndex)
        {
            var count = endIndex - startIndex;
            var text = new char[count];
            _sb.CopyTo(startIndex, text, 0, count);
            return new string(text);
        }

        public void Dispose()
        {
            if (_sb != null)
            {
                _sb.ClearAndFree();
                _sb = null;
                _length = 0;
            }
        }
    }
    
    public class SpellingCapture : ISpellingCapture
    {
        private string _spelling;
        private CharBuffer _charBuffer;
        private int _startIndex;
        private int _endIndex;

        private IDfaRecognition _capturedForRecognition;
        private IRecognizerCompletion _capturedForCompletion;

        public SpellingCapture(CharBuffer charBuffer, int startIndex, int endIndex, IDfaRecognition recognition, IRecognizerCompletion completion)
        {
            _charBuffer = charBuffer;
            _startIndex = startIndex;
            _endIndex = endIndex;

            _capturedForRecognition = recognition;
            _capturedForCompletion = completion;
        }

        public string Spelling
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

        public IExpr Expression { get { return _capturedForCompletion.CompletedValue as IExpr; } }
    }
}

