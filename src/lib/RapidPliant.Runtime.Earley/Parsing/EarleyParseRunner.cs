using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar;
using RapidPliant.Runtime.Earley.Grammar;
using RapidPliant.Runtime.Earley.Lexing;

namespace RapidPliant.Runtime.Earley.Parsing
{
    public class EarleyParseRunner
    {
        private EarleyParseEngine ParseEngine { get; set; }

        private LexemeProcessList ExistingLexemes { get; set; }

        private ScannedCapturesList ScannedCaptures { get; set; }

        public EarleyParseRunner(EarleyParseEngine parseEngine)
        {
            ParseEngine = parseEngine;
        }

        public void ProcessNext()
        {
            var c = ReadNext();

            var scannedAny = ScanNext(c);
        }

        private char ReadNext()
        {
            return '\0';
        }

        private bool ScanNext(char c)
        {
            var scannedAny = false;
            var existingLexemes = ExistingLexemes;
            var scannedCaptures = ScannedCaptures;

            //1. Each lexeme represents the LexRules from the expected transitions of an Earley DFA state
            //- therefor, once a lexeme ha some captures, these can be from different Lex rules and be ambigious

            //Begin scanning process for the lexemes list
            scannedCaptures.BeginScan();
            existingLexemes.BeginScan();
            while (existingLexemes.HasLexemesToScan)
            {
                var lexemeToScan = existingLexemes.NextLexemeToScan();

                scannedCaptures.Reset();
                //Scan the next lexeme!
                if (!lexemeToScan.Lexeme.Scan(c, scannedCaptures))
                {
                    //If we have any captures... we need to pulse for those!
                    if (scannedCaptures.HasCaptures)
                    {
                        //Parse next for the scanned captures!
                        //Note that doing this can change the "existing lexemes"... new lexemes can be added!
                        TryParseNext(scannedCaptures);
                    }

                    //Lexeme is done... can't proceed!
                    lexemeToScan.Dispose();
                    continue;
                }

                scannedAny = true;
            }
            //End the scan
            existingLexemes.EndScan();
            scannedCaptures.EndScan();
            
            
            return scannedAny;
        }

        private void TryParseNext(ScannedCapturesList captures)
        {
            //Do we process for all the captures, or only the longest/last?
            //- there can be multiple capture
            //- only a single capture per LexRule
            
            var capture = captures.LastCapture;
            var token = CreateTokenForCapture(capture);
            var endLocation = capture.EndLocation;

            ParseNext(token, endLocation);
        }

        private IToken CreateTokenForCapture(LexCapture capture)
        {
            return null;
        }

        private void ParseNext(IToken token, int atLocation)
        {
            ParseEngine.Pulse(atLocation, token, ExistingLexemes);
        }
    }

    public class LexemeProcessList
    {
        public bool HasLexemesToScan { get; set; }

        public void BeginScan()
        {
        }

        public void EndScan()
        {
        }
        
        public LexemeScan NextLexemeToScan()
        {
            return null;
        }

        public void RemoveAt(int index)
        {
            //Remove the lexeme at the specified index!
        }

        public void CollectFrom(EarleyState earleyState)
        {
            var lexRules = earleyState.DfaState.ScanTransitions.AllLexDefs;
            for (var i = 0; i < lexRules.Length; ++i)
            {
                var lexRule = lexRules[i];
                CreateNewLexemeIfNotExists(lexRule);
            }
        }

        private void CreateNewLexemeIfNotExists(IEarleyLexDef lexRule)
        {
            //Create a new lexeme for the specified lex rule if not already exists!
            var lexeme = lexRule.LexemeFactory.Create();
        }
    }

    public class LexemeScan
    {
        private LexemeProcessList _list;
        private int _index;

        public LexemeScan(LexemeProcessList list, int index)
        {
            _list = list;
            _index = index;
        }

        public ILexeme Lexeme { get; set; }

        public void Remove()
        {
            //Remove from the lexemes list!
        }

        public void Dispose()
        {
            _list.RemoveAt(_index);
            Lexeme.Dispose();
        }
    }

    public class ScannedCapturesList
    {
        public void BeginScan()
        {
        }

        public void EndScan()
        {
        }

        public void Reset()
        {
        }

        public bool HasCaptures { get; set; }

        public LexCapture LastCapture { get; set; }
    }
}
