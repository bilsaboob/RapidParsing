using System;
using System.Diagnostics;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Text;
using RapidPliant.Parsing.Earley.HandRolled2;

namespace RapidPliant.RapidBnf.Test.Tests.Grammar
{
    public class RapidBnf_Handrolled_ParseTestBase : RapidBnfParseTestBase
    {
        protected ParseStats Parse(string inputText)
        {
            var input = new StringBuffer(inputText);
            var lexer = RapidBnfGrammar.CreateLexer();
            var stats = new ParseStats();

            Parse(lexer, input, stats);

            return stats;
        }

        protected void ParseRepeated(string inputText, int repeatCount)
        {
            var input = new StringBuffer(inputText);
            var lexer = RapidBnfGrammar.CreateLexer();

            var totalStats = new ParseStats();

            for (var i = 0; i < repeatCount; ++i)
            {
                Parse(lexer, input, totalStats);
            }

            PrintStats(totalStats, repeatCount);
        }

        protected void ParseUntilUserInput(string inputText)
        {
            var input = new StringBuffer(inputText);
            var lexer = RapidBnfGrammar.CreateLexer();

            while (true)
            {
                var stats = new ParseStats();

                Parse(lexer, input, stats);

                PrintStats(stats);

                if (Console.ReadLine() != "")
                    break;
            }
        }
        
        protected ParseStats Parse(Lexer lexer, IBuffer input, ParseStats stats)
        {
            stats.Start();

            // init the lexer with the input
            lexer.Init(input);

            // create the tokens stream by reading all the tokens up front
            var tokens = LexTokens(lexer, stats);

            // prep the parsing context
            var context = new ParseContext(tokens);

            if (!RapidBnfGrammar.ParseGrammar(context.Start()))
            {
                // error
            }
            else
            {
                // success
            }

            stats.Parsed();

            return stats;
        }

        protected BufferTokenStream LexTokens(Lexer lexer, ParseStats stats)
        {
            var tokens = new BufferTokenStream(new LexerTokenStream(lexer).ReadAllTokens());
            /*var badTokensCount = tokens.Tokens.Where(t => t.IsBadToken).ToList();
            foreach (var token in badTokensCount)
            {
                var text = input.GetText(token.Range());
            }

            foreach (var token in tokens.Tokens)
            {
                var text = input.GetText(token.Range());
            }*/
            tokens.Reset();
            stats.Lexed();
            return tokens;
        }
    }
}
