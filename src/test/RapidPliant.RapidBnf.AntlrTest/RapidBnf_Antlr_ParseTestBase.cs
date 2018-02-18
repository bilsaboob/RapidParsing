using System;
using Antlr4.Runtime;
using RapidPliant.RapidBnf.Test.Tests.Grammar;

namespace AntlrTest
{
    public class RapidBnf_Antlr_ParseTestBase : RapidBnfParseTestBase
    {
        protected ParseStats Parse(string text)
        {
            var inputStream = new AntlrInputStream(text);
            var lexer = new JSONLexer(inputStream);
            var stats = new ParseStats();

            Parse(lexer, stats);

            return stats;
        }

        protected void ParseRepeated(string inputText, int repeatCount)
        {
            var inputStream = new AntlrInputStream(inputText);
            var lexer = new JSONLexer(inputStream);

            var totalStats = new ParseStats();

            for (var i = 0; i < repeatCount; ++i)
            {
                Parse(lexer, totalStats);
            }

            PrintStats(totalStats, repeatCount);
        }

        protected void ParseUntilUserInput(string inputText)
        {
            var inputStream = new AntlrInputStream(inputText);
            var lexer = new JSONLexer(inputStream);

            while (true)
            {
                var stats = new ParseStats();

                Parse(lexer, stats);

                PrintStats(stats);

                if (Console.ReadLine() != "")
                    break;
            }
        }
        
        protected void Parse(JSONLexer lexer, ParseStats stats)
        {
            stats.Start();

            // lex all tokens beforehand
            lexer.Reset();
            var tokens = lexer.GetAllTokens();
            var tokenStream = new CommonTokenStream(new ListTokenSource(tokens));

            stats.Lexed();

            //var tokenStream = new CommonTokenStream(lexer);
            var parser = new JSONParser(tokenStream);
            parser.BuildParseTree = false;
            var result = parser.json();

            stats.Parsed();
            stats.End();
        }
    }
}