using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar.Definitions;

namespace RapidPliant.Grammar
{
    public class SimpleJsonGrammarModel : SimpleGrammarModel<SimpleJsonGrammarModel>
    {
        public RuleDef
            Json = "Json",
            Object = "Object",
            Pair = "Pair",
            PairRepeat = "PairRepeat",
            Array = "Array",
            Value = "Value",
            ValueRepeat = "ValueRepeat"
            ;

        public LexDef
            number = "number",
            strQuote = "quoted_string"
            ;

        protected override void Define()
        {
            //Configure the lex declarations
            number.As(Number());
            strQuote.As(StringQuoted());

            //Configure the rule declarations
            Json.As(
                Object | Value
            );
            
            Object.As(
                '{' + Value + '}'
            );
            
            Value.As(
                strQuote | number
            );
            
            //Set the start rule!
            Start(Json);
        }
    }
}
