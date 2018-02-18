using System;
using Antlr4.Runtime;
using RapidPliant.RapidBnf.Test.Tests.Grammar;

namespace AntlrTest
{
    public abstract class RapidBnf_Antlr_ParseTestBase : RapidBnfParseTestBase
    {
        protected ParseStats Parse(string text)
        {
            var inputStream = new AntlrInputStream(text);
            var lexer = CreateLexer(inputStream);
            var stats = new ParseStats();

            Parse(lexer, stats);

            return stats;
        }

        protected void ParseRepeated(string inputText, int repeatCount)
        {
            var inputStream = new AntlrInputStream(inputText);
            var lexer = CreateLexer(inputStream);

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
            var lexer = CreateLexer(inputStream);

            while (true)
            {
                var stats = new ParseStats();

                Parse(lexer, stats);

                PrintStats(stats);

                if (Console.ReadLine() != "")
                    break;
            }
        }
        
        protected virtual void Parse(Lexer lexer, ParseStats stats)
        {
            stats.Start();

            // lex all tokens beforehand
            lexer.Reset();
            var tokens = lexer.GetAllTokens();
            var tokenStream = new CommonTokenStream(new ListTokenSource(tokens));

            stats.Lexed();

            //var tokenStream = new CommonTokenStream(lexer);
            var parser = CreateParser(tokenStream);
            parser.BuildParseTree = false;
            var result = Parse(parser);

            stats.Parsed();
            stats.End();
        }

        protected abstract Lexer CreateLexer(AntlrInputStream inputStream);

        protected abstract Parser CreateParser(CommonTokenStream tokenStream);

        protected abstract object Parse(Parser parser);
    }
}