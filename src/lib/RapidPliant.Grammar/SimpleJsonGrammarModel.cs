using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public class SimpleJsonGrammarModel : SimpleGrammarModel<SimpleJsonGrammarModel>
    {
        public RuleExpr
            Json = "Json",
            Object = "Object",
            Pair = "Pair",
            PairRepeat = "PairRepeat",
            Array = "Array",
            Value = "Value",
            ValueRepeat = "ValueRepeat"
            ;

        public LexExpr
            number = "number",
            strQuote = "quoted_string"
            ;

        protected override void Define()
        {
            //Configure the lex declarations
            number = Number("number");
            strQuote = StringQuoted("quoted_string");

            //Configure the rule declarations
            Json.As(
                Object
            );

            Object.As(
                '{' + Value + '}'
            );
            
            Value.As(
                strQuote | number
            );

            var val = Value.ToString();

            //Set the start rule!
            Start(Json);
        }
    }
}
