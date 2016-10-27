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
    public class DfaLexer : IDisposable
    {
        private DfaGraph _dfa;
        private DfaState _startState;
        private DfaState _state;
        private bool _canContinue;

        private CharBuffer _spelling;
        
        private List<SpellingCapture> _captures;
        private List<SpellingCapture> _scannedCaptures;

        public DfaLexer(DfaGraph dfa)
        {
            _dfa = dfa;
            _startState = dfa.StartState;
            
            Reset();
        }

        public bool CanContinue { get { return _canContinue; } }

        public IReadOnlyList<SpellingCapture> Captures { get { return _captures; } }

        public IReadOnlyList<SpellingCapture> ScannedCaptures { get { return _scannedCaptures; } }
        
        private void Reset()
        {
            _state = _startState;
            _spelling = new CharBuffer(ReusableStringBuilder.GetAndClear());
            _captures = ReusableList<SpellingCapture>.GetAndClear();
            _scannedCaptures = ReusableList<SpellingCapture>.GetAndClear();

            _canContinue = true;
        }
        
        public bool Scan(char ch)
        {
            //Clear the scanned captures
            _scannedCaptures.Clear();

            var t = GetTransition(ch);
            if (t == null)
            {
                //No more valid transitions!
                _canContinue = false;
                return false;
            }

            //Append the spelling
            _spelling.Append(ch);

            //Move to the next state
            _state = t.ToState;

            Capture(t);

            return true;
        }
        
        private DfaTransition GetTransition(char ch)
        {
            //Find the transition for the specified char
            var transitions = _state.Transitions;
            var transitionsCount = transitions.Count;
            for (var i = 0; i < transitionsCount; ++i)
            {
                var transition = transitions[i];

                var interval = transition.Interval;
                if (interval == null)
                    continue;

                if(ch < interval.Min)
                    continue;

                if(ch > interval.Max)
                    continue;

                return transition;
            }

            return null;
        }

        private void Capture(DfaTransition transition)
        {
            if (transition == null)
                return;
            
            var completionsByExpressions = transition.CompletionsByExpression;
            if(completionsByExpressions == null)
                return;

            foreach (var exprCompletion in completionsByExpressions)
            {
                AddCapture(transition, exprCompletion);
            }
        }

        private void AddCapture(DfaTransition transition, DfaCompletion completion)
        {
            var capture = new SpellingCapture(_spelling, 0, _spelling.Length, transition, completion);
            _captures.Add(capture);
            _scannedCaptures.Add(capture);
        }

        public void Dispose()
        {
            if (_spelling != null)
            {
                _spelling.Dispose();
            }

            if (_captures != null)
            {
                _captures.ClearAndFree();
            }

            if (_scannedCaptures != null)
            {
                _scannedCaptures.ClearAndFree();
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

    public class SpellingCapture
    {
        private string _spelling;
        private CharBuffer _charBuffer;
        private int _startIndex;
        private int _endIndex;

        private DfaTransition _capturedForTransition;
        private DfaCompletion _capturedForCompletion;

        public SpellingCapture(CharBuffer charBuffer, int startIndex, int endIndex, DfaTransition transition, DfaCompletion completion)
        {
            _charBuffer = charBuffer;
            _startIndex = startIndex;
            _endIndex = endIndex;

            _capturedForTransition = transition;
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

        public IExpr Expression { get { return _capturedForCompletion.Expression; } }
    }
}

