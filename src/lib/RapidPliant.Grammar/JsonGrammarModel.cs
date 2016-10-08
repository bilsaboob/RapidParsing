using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar.Definitions;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public class JsonGrammarModel : SimpleGrammarModel<JsonGrammarModel>
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
                Value
            );

            Object.As(
                '{' + PairRepeat + '}'
            );

            PairRepeat.As(
                Pair
                | Pair + ',' + PairRepeat
                | Null
            );

            Pair.As(
                strQuote + ':' + Value
            );

            Array.As(
                '[' + ValueRepeat + ']'
            );

            ValueRepeat.As(
                Value
                | Value + ',' + ValueRepeat
                | Null
            );

            Value.As(
                strQuote
                | number
                | Object
                | Array
                | "true"
                | "false"
                | "null"
            );

            Start(Json);
        }
    }
}
