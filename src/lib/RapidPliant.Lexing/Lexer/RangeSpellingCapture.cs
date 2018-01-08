using RapidPliant.Automata.Dfa;
using RapidPliant.Common.Expression;

namespace RapidPliant.Lexing.Lexer
{
    public class RangeSpellingCapture : ISpellingCapture
    {
        protected int _startIndex;
        protected int _endIndex;

        protected IDfaRecognition _capturedForRecognition;
        protected IRecognizerCompletion _capturedForCompletion;

        private IExpr _completedExpr;

        public RangeSpellingCapture(int startIndex, int endIndex, IDfaRecognition recognition, IRecognizerCompletion completion)
        {
            _startIndex = startIndex;
            _endIndex = endIndex;

            _capturedForRecognition = recognition;
            _capturedForCompletion = completion;
        }

        public virtual string Spelling => null;

        public int Start => _startIndex;

        public int End => _endIndex;

        public IExpr Expression
        {
            get
            {
                if (_completedExpr == null)
                {
                    _completedExpr = _capturedForCompletion.CompletedValue as IExpr;
                }

                return _completedExpr;
            }
        }
    }
}