using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Runtime.Earley.Automata.Lex;
using RapidPliant.Runtime.Earley.Parsing;
using RapidPliant.Runtime.Util;

namespace RapidPliant.Runtime.Earley.Lexing
{
    public interface ILexeme
    {
        bool Scan(char c, ScannedCapturesList capturesList);

        void Dispose();
    }

    public interface ILexemeFactory
    {
        ILexeme Create();
    }

    public class LexDfaLexemeFactory
    {
        private ILexDfa LexDfa { get; set; }

        public LexDfaLexemeFactory(ILexDfa lexDfa)
        {
            LexDfa = lexDfa;
        }

        public ILexeme Create()
        {
            return new LexDfaLexeme(LexDfa);
        }
    }

    public class LexDfaLexeme : ILexeme
    {
        private ILexDfa LexDfa { get; set; }

        private ILexDfaState State { get; set; }

        private LexCaptureList Captures { get; set; }

        public LexDfaLexeme(ILexDfa lexDfa)
        {
            LexDfa = lexDfa;

            State = LexDfa.StartState;
        }

        public bool Scan(char c, ScannedCapturesList capturesList)
        {
            var t = State.Transitions.Get(c);

            var toState = t.ToState;
            
            //Check for completions - we will 
            if (toState.HasCompletions)
            {
                //Start a new capture for each of the completions...
                var completions = toState.Completions;
                for (var i = 0; i < completions.Length; ++i)
                {
                    var completion = completions[i];
                    Captures.BeginCaptureIfNotExists(completion);
                }
            }

            return false;
        }

        public void Dispose()
        {
        }
    }

    public class LexCaptureList
    {
        private RapidLinkedList<LexCapture> _captures;

        public LexCaptureList()
        {
            _captures = new RapidLinkedList<LexCapture>();
        }

        public void BeginCaptureIfNotExists(ILexCompletion completion)
        {
            var lexNfa = completion.LexNfa;

            //Use the local index to lookup
            var index = lexNfa.LocalIndex;
            
            var capture = _captures[index];
            if (capture == null)
            {
                capture = new LexCapture(completion);
                _captures[index] = capture;
            }
        }
    }

    public class LexCapture
    {
        private ILexCompletion _completion;

        public LexCapture(ILexCompletion completion)
        {
            _completion = completion;
        }

        public int EndLocation { get; set; }
    }
}
