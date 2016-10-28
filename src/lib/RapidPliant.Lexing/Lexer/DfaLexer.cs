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
    public interface IDfaRecognizer
    {
    }

    public interface IDfaRecognizer<TInput> : IDfaRecognizer
    {
        IDfaRecognition Recognize(TInput input);

        void Reset();
    }

    public interface IDfaRecognition
    {
        DfaState FromState { get; }
        DfaState ToState { get; }
        IEnumerable<IRecognizerCompletion> Completions { get; }
    }

    public interface IRecognizerCompletion
    {
        object CompletionInfo { get; }
        object CompletedValue { get; }
    }
    
    public class DfaTableRecognizer<TInput> : IDfaRecognizer<TInput>
    {
        public DfaTableRecognizer(DfaGraph dfaGraph)
        {
            DfaGraph = dfaGraph;

            Build();
        }
        
        protected DfaGraph DfaGraph { get; set; }

        private void Build()
        {
        }

        public IDfaRecognition Recognize(TInput input)
        {
            return null;
        }

        public void Reset()
        {
        }
    }

    public class DfaLexer : IDisposable
    {
        private CharBuffer _spelling;

        protected List<SpellingCapture> _captures;
        protected List<SpellingCapture> _scannedCaptures;

        private bool _canContinue;

        protected IDfaRecognizer<char> _recognizer;
        
        public DfaLexer(IDfaRecognizer<char> recognizer)
        {
            _recognizer = recognizer;
            
            Reset();
        }

        public bool CanContinue { get { return _canContinue; } }
        
        public IReadOnlyList<SpellingCapture> Captures { get { return _captures; } }

        public IReadOnlyList<SpellingCapture> ScannedCaptures { get { return _scannedCaptures; } }

        public void Init()
        {
            Reset();
        }

        private void Reset()
        {
            _recognizer.Reset();

            _spelling = new CharBuffer(ReusableStringBuilder.GetAndClear());
            _captures = ReusableList<SpellingCapture>.GetAndClear();
            _scannedCaptures = ReusableList<SpellingCapture>.GetAndClear();

            _canContinue = true;
        }

        public bool Lex(char ch)
        {
            _scannedCaptures.Clear();

            var r = _recognizer.Recognize(ch);
            if (r == null)
            {
                //No more valid recognition!
                _canContinue = false;
                return false;
            }

            //Append the spelling
            _spelling.Append(ch);

            Capture(r);

            return true;
        }

        private void Capture(IDfaRecognition recognition)
        {
            if(recognition == null)
                return;

            var completions = recognition.Completions;
            if(completions == null)
                return;
            
            foreach (var completion in completions)
            {
                AddCapture(recognition, completion);
            }
        }

        private void AddCapture(IDfaRecognition recognition, IRecognizerCompletion completion)
        {
            var capture = new SpellingCapture(_spelling, 0, _spelling.Length, recognition, completion);
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

