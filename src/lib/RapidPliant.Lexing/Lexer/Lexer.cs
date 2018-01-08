using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Lexer
{
    public interface ILexer
    {
	    void Init();
        bool Lex(ILexContext context);
	    bool CanContinue { get; }
    }

    public interface ILexContext
    {
        char CharToLex { get; }
        
        void AddCapture(ISpellingCapture spellingCapture);
    }

    public interface ISpellingCapture
    {
        string Spelling { get; }

        IExpr Expression { get; }
    }

    public class LexContext : ILexContext, IDisposable
    {
        protected List<ISpellingCapture> _captures;

        public LexContext()
        {
            _captures = ReusableList<ISpellingCapture>.GetAndClear();
        }

        public char CharToLex { get; set; }

        public IReadOnlyList<ISpellingCapture> Captures { get { return _captures; } }
        
        public void AddCapture(ISpellingCapture spellingCapture)
        {
            _captures.Add(spellingCapture);
        }

        public void ClearCaptures()
        {
            _captures.Clear();
        }

        public void Dispose()
        {
            if (_captures != null)
            {
                _captures.ClearAndFree();
                _captures = null;
            }
        }
    }
}
