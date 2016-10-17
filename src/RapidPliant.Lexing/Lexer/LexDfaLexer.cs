using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Util;
using RapidPliant.Lexing.Dfa;
using RapidPliant.Lexing.Pattern;

namespace RapidPliant.Lexing.Lexer
{
    public class LexDfaTableLexer
    {
        private LexDfa _dfa;
        private LexDfaTableState _startState;
        private LexDfaTableState _state;

        private CachingRapidList<SpellingCapture> _captures;
        private RapidList<SpellingCapture> _scannedCaptures;

        private CharBuffer _spelling;

        public LexDfaTableLexer(LexDfa dfa)
        {
            _dfa = dfa;
            _startState = new LexDfaTableState(_dfa.StartState);
            _state = _startState;
            _captures = new CachingRapidList<SpellingCapture>();
            _spelling = new CharBuffer(new StringBuilder());
            _scannedCaptures = new CachingRapidList<SpellingCapture>();
        }

        public int ScannedCapturesCount { get { return _scannedCaptures.Count; } }
        public IEnumerable<SpellingCapture> ScannedCaptures { get { return _scannedCaptures; } }

        public int CapturesCount { get { return _captures.Count; } }
        public SpellingCapture[] Captures { get { return _captures.AsArray; } }

        public bool CanContinue { get; private set; }

        public bool Scan(char ch)
        {
            var fromState = _state;
            _scannedCaptures.Clear();

            var t = fromState.GetTransition(ch);
            if (t == null)
            {
                //No more valid transitions!
                CanContinue = false;
                return false;
            }

            //Append the spelling
            _spelling.Append(ch);

            _state = t.ToState;
            
            Capture(t);

            return false;
        }

        private void Capture(LexDfaTableStateTransition transition)
        {
            if(transition == null)
                return;

            var dfaTransition = transition.DfaTransition;
            var completionsCount = dfaTransition.CompletionsCount;
            if (completionsCount == 0)
                return;

            var completionsByRule = dfaTransition.CompletionsByRule;
            for (var i = 0; i < completionsCount; ++i)
            {
                var ruleCompletion = completionsByRule[i];
                AddCapture(transition, ruleCompletion);
            }
        }

        private void AddCapture(LexDfaTableStateTransition transition, LexDfaStateCompletion completion)
        {
            var capture = new SpellingCapture(_spelling, 0, _spelling.Length, transition, completion);
            _captures.Add(capture);
            _scannedCaptures.Add(capture);
        }
    }

    public class CharBuffer
    {
        private StringBuilder _sb;

        public CharBuffer(StringBuilder sb)
        {
            _sb = sb;
        }

        public int Length { get { return _sb.Length; } }

        public void Append(char ch)
        {
            _sb.Append(ch);
        }

        public string GetText(int startIndex, int endIndex)
        {
            var count = endIndex - startIndex;
            var text = new char[count];
            _sb.CopyTo(startIndex, text, 0, count);
            return new string(text);
        }
    }

    public class SpellingCapture
    {
        private string _spelling;
        private CharBuffer _charBuffer;
        private int _startIndex;
        private int _endIndex;

        private LexDfaTableStateTransition _capturedForTransition;
        private LexDfaStateCompletion _capturedForCompletion;

        public SpellingCapture(CharBuffer charBuffer, int startIndex, int endIndex, LexDfaTableStateTransition transition, LexDfaStateCompletion completion)
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
    }

    public class LexDfaTableState
    {
        private int _minCharValue;
        private int _maxCharValue;
        private LexDfaTableStateTransition[] _transitionsByChar;

        public LexDfaTableState(LexDfaState dfaState)
        {
            DfaState = dfaState;
        }

        public LexDfaState DfaState { get; set; }
        
        public LexDfaTableStateTransition GetTransition(char ch)
        {
            if (ch < _minCharValue)
                return null;

            if (ch > _maxCharValue)
                return null;

            return _transitionsByChar[ch];
        }

        public void Compile()
        {
            //Currently we only allow UTF8 charset- 128 is the "default"... 256 to support the extended...
            _minCharValue = 0;
            _maxCharValue = 255;
            _transitionsByChar = new LexDfaTableStateTransition[_maxCharValue+1];

            var dfaTermTransitions = DfaState.GetTermTransitions();
            foreach (var dfaTermTransition in dfaTermTransitions)
            {
                AddDfaTermTransition(dfaTermTransition);
            }
        }

        private void AddDfaTermTransition(LexDfaStateTermTransition dfaTermTransition)
        {
            var termSymbol = dfaTermTransition.TransitionTermSymbol;

            var termSingleCharSymbol = termSymbol as ILexPatternTerminalCharSymbol;
            if (termSingleCharSymbol != null)
            {
                //Create the table transition for single character
                var ch = termSingleCharSymbol.Char;
                _transitionsByChar[ch] = new LexDfaTableStateTransition(this, ch, dfaTermTransition);
                return;
            }

            var termRangeSymbol = termSymbol as ILexPatternTerminalRangeSymbol;
            if (termRangeSymbol != null)
            {
                //Creat the table transitions for all the expected characters
                var fromChar = (int)termRangeSymbol.FromChar;
                var toChar = (int)termRangeSymbol.ToChar;
                for (var i = fromChar; i < toChar; ++i)
                {
                    var ch = (char) i;
                    _transitionsByChar[ch] = new LexDfaTableStateTransition(this, ch, dfaTermTransition);
                }
            }
        }
    }

    public class LexDfaTableStateTransition
    {
        private LexDfaStateTermTransition _dfaTermTransition;

        public LexDfaTableStateTransition(LexDfaTableState fromTableState, char transitionChar, LexDfaStateTermTransition dfaTermTransition)
        {
            FromState = fromTableState;

            _dfaTermTransition = dfaTermTransition;
            Char = transitionChar;
        }

        public char Char { get; private set; }

        public LexDfaTableState FromState { get; private set; }

        public LexDfaTableState ToState { get; private set; }

        public LexDfaStateTermTransition DfaTransition { get { return _dfaTermTransition; } }
    }
}

