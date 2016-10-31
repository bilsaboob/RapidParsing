using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Parsing.Earley.Recognition;

namespace RapidPliant.Parsing.Earley
{
    public class EarleyParser : IEarleyParser
    {
        public EarleyParser(EarleyGrammar grammar)
        {
            Grammar = grammar;
        }

        public EarleyGrammar Grammar { get; private set; }

        public IEarleyRecognizer Recognizer { get; private set; }
        public EarleyEngine Earley { get; private set; }

        public TextReader Input { get; private set; }
        
        public void Begin(TextReader input)
        {
            Input = input;

            //Ensure the grammar is built
            Grammar.EnsureBuild();
            
            //Create a new recognizer
            Recognizer = new EarleyDfaStateTransitionRecognizer(Grammar.EarleyDfa);

            //Create the earley engine
            Earley = new EarleyEngine(Recognizer);
        }

        public void Parse(TextReader input)
        {
            Begin(input);

            while (true)
            {
                var ch = Input.Read();
                if (ch == -1)
                {
                    //End of input
                    break;
                }

                //Scan for the read input
                ScanNext((char)ch);
            }
        }

        private void ScanNext(char ch)
        {
            //use the active lexemes
        }
    }
}
