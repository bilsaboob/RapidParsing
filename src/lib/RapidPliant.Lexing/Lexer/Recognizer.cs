using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Automata;

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
        IReadOnlyList<IRecognizerCompletion> Completions { get; }
    }

    public interface IRecognizerCompletion
    {
        object CompletionInfo { get; }
        object CompletedValue { get; }
    }
}
