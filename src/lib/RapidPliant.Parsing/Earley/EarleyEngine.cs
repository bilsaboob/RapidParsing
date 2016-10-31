using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Parsing.Earley.Recognition;

namespace RapidPliant.Parsing.Earley
{
    public class EarleyEngine
    {
        public EarleyEngine(IEarleyRecognizer recognizer)
        {
        }

        public IEarleyRecognizer Recognizer { get; protected set; }

        public bool Parse(IParseContext parseContext)
        {
            var token = parseContext.TokenToParse;

            var id = token.Id;

            //Let the recognzer prase the next
            var r = Recognizer.Recognize(id);
            if (r == null)
            {
                return false;
            }

            return true;
        }
    }
}
