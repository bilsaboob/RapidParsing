using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Text;
using RapidPliant.Parsing.RecursiveDescent;
using RapidPliant.Parsing.RecursiveDescent.RapidBnf;

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

                GarbageCollect();
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

                WithDisabledGarbageCollection(1000 * 10000, () => {
                    Parse(lexer, input, stats);

                    PrintStats(stats);
                });

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

            var success = RapidBnfGrammar.ParseGrammar(context.Start());
            if (success)
            {
                //Console.WriteLine("Success");
                // success
                var parseTree = context.ParseTree;
                var parseNode = context.ParseNode;
                var nodesCount = parseTree.Count;
                parseTree.Dispose();
            }
            else
            {
                //Console.WriteLine("Error");
                // error
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
