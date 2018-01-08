using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Lexer.Recognition;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Lexing.Text;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Lexer
{
    public class SingleCaptureDfaTokenizer : ITokenizer, IDisposable
    {
        private ILexRecognizer _recognizer;

        private bool _canContinue;

        private int _startIndex;
        private int _index;
        
        public SingleCaptureDfaTokenizer(ILexRecognizer recognizer)
        {
            _recognizer = recognizer;
        }

        public bool CanContinue => _canContinue;
        
        public void Init(int index)
        {
            _index = index;
            _startIndex = _index;
            _canContinue = true;
            _recognizer.Reset();
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

            // move to the next position
            _index++;

            //Only pick the last completion
            var completions = r.Completions;
            if (completions != null && completions.Count > 0)
            {
                context.Capture = new RangeSpellingCapture(_startIndex, _index, r, completions[completions.Count - 1]);
            }

            return true;
        }

        public void Dispose()
        {    
        }
    }
}

