﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Common.Rule;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Automata.Dfa;
using RapidPliant.Lexing.Automata.Nfa;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Lexer.Recognition;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Lexing.Pattern.Regex;
using RapidPliant.Test;
using RapidPliant.Util;

namespace RapidPliant.Testing.Tests
{
    public class RegexPatternTest : TestBase
    {
        private RapidRegex Regex = new RapidRegex();

        protected override void Test()
        {
            //TestSingle();
            //TestMerged();
            TestMerged2();
        }

        private List<int> GenerateItems(int count)
        {
            var items = new List<int>();
            for (var i = 1; i <= count; ++i)
            {
                items.Add(i);
            }
            return items;
        }

        private void TestMerged2()
        {
            var dfaGraph = CreateLexDfa(
                CreateLexExpr("[0-9]*", "NUMBER"),
                CreateLexExpr("([a-z]|[A-Z]|_)([a-z]|[A-Z]|_|[0-9])*", "IDENTIFIER"),

                CreateLexExpr("public", "PUBLIC"),
                CreateLexExpr("static", "STATIC"),
                CreateLexExpr("class", "CLASS"),
                
                CreateLexExpr("def", "DEF"),

                CreateLexExpr("{", "LB"),
                CreateLexExpr("}", "RB"),
                CreateLexExpr("\\(", "LP"),
                CreateLexExpr("\\)", "RP")
            );

            /*TestLexing(dfaGraph, "public");
            TestLexing(dfaGraph, "static");
            TestLexing(dfaGraph, "class");
            TestLexing(dfaGraph, "10593");
            TestLexing(dfaGraph, "MyCoolName");
            TestLexing(dfaGraph, "MyCool_Name123_test");
            TestLexing(dfaGraph, "_SomeName");
            TestLexing(dfaGraph, "1_SomeName");*/

            //TestLexInput(dfaGraph, @"public class Test { def MyCoolMethod() {} }");

            TestLexFile(dfaGraph, @"testfiles\test.txt");
        }
        
        class TestToken
        {
            public TestToken(string name, string spelling, int line, int col, int endLine, int endCol)
            {
                Name = name;
                Spelling = spelling;
                Line = line;
                Col = col;
                EndLine = endLine;
                EndCol = endCol;
            }

            public string Name { get; set; }
            public string Spelling { get; set; }
            public int Line { get; set; }
            public int Col { get; set; }
            public int EndLine { get; set; }
            public int EndCol { get; set; }

            public override string ToString()
            {
                return $"({Line}:{Col})({Name}:{Spelling})";
            }
        }

        private void TestLexFile(DfaGraph dfaGraph, string filePath)
        {
            var input = File.ReadAllText(filePath);
            var recognizer = new LexDfaTableTransitionRecognizer(dfaGraph);
            var lexer = new DfaLexer(recognizer);
            
            var count = 100;
            var sw = new Stopwatch();
            
            for (var i = 0; i < count; ++i)
            {
                var inputReader = new StringReader(input);

                sw.Start();
                TestLexInput(lexer, inputReader);
                sw.Stop();
            }
            
            Console.WriteLine($"{sw.ElapsedMilliseconds/count}");

            /*foreach (var token in tokens)
            {
                Console.Write(token.Spelling + " ");
            }*/
        }

        private void TestLexInput(DfaGraph dfaGraph, string input)
        {
            var recognizer = new LexDfaTableTransitionRecognizer(dfaGraph);
            var lexer = new DfaLexer(recognizer);
            var inputReader = new StringReader(input);
            TestLexInput(lexer, inputReader);
        }

        private List<TestToken> TestLexInput(DfaLexer lexer, StringReader inputReader)
        {
            var tokens = new List<TestToken>();
            var allCaptures = new List<ISpellingCapture>();
            var lexContext = new LexContext();

            var lineNo = 0;
            var colNo = 0;

            // init as whitespace
            char ch = ' ';
            int i = ch;
            int tokenStartLine = lineNo;
            int tokenStartCol = colNo;
            
            while (true)
            {
                var success = false;
                allCaptures.Clear();
                lexContext.ClearCaptures();
                lexer.Init();

                ISpellingCapture lastCapture = null;
                
                // skip whitepace before
                while (true)
                {
                    if (!char.IsWhiteSpace(ch))
                        break;

                    if (ch == '\n')
                    {
                        lineNo++;
                        colNo = 0;
                    }

                    i = inputReader.Read();
                    ch = (char)i;

                    colNo++;
                }

                while (true)
                {
                    //Prepare the lex context for the next lex
                    
                    lexContext.CharToLex = ch;
                    lexContext.ClearCaptures();

                    //Lex!
                    lexer.Lex(lexContext);

                    //Check the captures that were captured for this lex pass
                    //There can be multiple captures with same spelling, in case of multiple rules that match the same input
                    var capture = lexContext.Captures.LastOrDefault();
                    if (capture != null)
                        lastCapture = capture;

                    if(!lexer.CanContinue)
                        break;

                    i = inputReader.Read();
                    ch = (char)i;

                    if (ch == '\n')
                    {
                        lineNo++;
                        colNo = 0;
                    }
                    else
                    {
                        colNo++;
                    }

                    if (i == -1)
                    {
                        //TODO: currently no captures... but if we get here... it means we have successfully recognized the input up until this point...
                        success = true;
                        break;
                    }
                }

                if (success)
                {
                    break;
                }

                if (lastCapture != null)
                {
                    var name = lastCapture.Expression.Root.Name;
                    var spelling = lastCapture.Spelling;
                    tokens.Add(new TestToken(name, spelling, tokenStartLine, tokenStartCol, lineNo, colNo-1));
                }
                else
                {
                    // failed to read toke
                    throw new Exception("Failed to read token");
                }

                // skip whitepace after new token
                while (true)
                {
                    if (!char.IsWhiteSpace(ch))
                        break;

                    if (ch == '\n')
                    {
                        lineNo++;
                        colNo = 0;
                    }

                    i = inputReader.Read();
                    ch = (char)i;

                    colNo++;
                }

                tokenStartLine = lineNo;
                tokenStartCol = colNo;
            }
            
            return tokens;
        }

        private void TestMerged()
        {
            var dfaGraph = CreateLexDfa(
                CreateLexExpr("abc(de)*", "A"),
                CreateLexExpr("abcd(ef)*", "B")
            );

            //Expect only A as full capture
            TestLexing(dfaGraph, "abc");

            //Expect A as a partial capture, "abc"
            //Expect B as a full capture, "abcd"
            TestLexing(dfaGraph, "abcd");

            //Expect A as a partial capture, twice, "abc" and "abcde"
            //Expect B as a partial capture, "abcd"
            TestLexing(dfaGraph, "abcde");
        }
        
        private void TestSingle()
        {
            //var expr = CreateLexExpr("a(bc|bd)*e");
            var expr = CreateLexExpr("a([b-e])*bekj");

            var nfa = LexNfaAutomata.BuildNfa(expr);
            var nfaGraph = nfa.ToNfaGraph();

            var dfa = LexDfaAutomata.BuildDfa(nfaGraph);
            var dfaGraph = dfa.ToDfaGraph();

            //Should pass:
            var passed = TestLexing(dfaGraph, "abbbbbeeeeecccccddddeeeeebekj");

            //Should fail:
            //var passed = TestLexing(dfaGraph, "abbbbbeeeeecccccddddeeeeebekkj");

            //Should pass:
            //var passed = TestLexing(dfaGraph, "abecdbekj");
        }


        private bool TestLexing(DfaGraph dfaGraph, string input)
        {
            //Lex using dfault state transition recognizer
            return TestLexing(dfaGraph, input, new LexDfaTableTransitionRecognizer(dfaGraph));
            //return TestLexing(dfaGraph, input, new LexDfaStateTransitionRecognizer(dfaGraph));
        }

        private bool TestLexing(DfaGraph dfaGraph, string input, ILexRecognizer recognizer)
        {
            var lexer = new DfaLexer(recognizer);
            var inputReader = new StringReader(input);
            var success = false;

            var allCaptures = new List<ISpellingCapture>();

            var lexContext = new LexContext();
            while (lexer.CanContinue)
            {
                var i = inputReader.Read();
                if (i == -1)
                {
                    //TODO: currently no captures... but if we get here... it means we have successfully recognized the input up until this point...
                    success = true;
                    break;
                }
                
                //Prepare the lex context for the next lex
                var ch = (char) i;
                lexContext.CharToLex = ch;
                lexContext.ClearCaptures();

                //Lex!
                lexer.Lex(lexContext);

                //Check the captures that were captured for this lex pass
                //There can be multiple captures with same spelling, in case of multiple rules that match the same input
                foreach (var capture in lexContext.Captures)
                {
                    var spelling = capture.Spelling;
                    allCaptures.Add(capture);
                }
            }

            //TODO: currently no captures...
            //Check all the captures
            foreach (var capture in allCaptures)
            {
                //The "expr" is the "leaf" expression that triggere the capture
                var expr = capture.Expression;
                var exprName = expr.Name;

                //The "rootExpr" is the "pattern expression" that we defined above, and has the name that we specified... the "expr" is a subexpression to the root... at some unknown level...
                //In general the "rootExpr" is of interest... it may have interesting configuration such as "Trigger Action XYZ" / "Token Factory that creates expecte token" etc...
                var rootExpr = expr.Root;
                var rootExprName = rootExpr.Name;

                //The spelling is the captured text
                var spelling = capture.Spelling;
            }

            return success;
        }
    
        protected RegexExpr CreateLexExpr(string regexPattern, string name = null)
        {
            var expr = Regex.FromPattern(regexPattern);
            if (!string.IsNullOrEmpty(name))
                expr.Name = name;
            return expr;
        }

        protected DfaGraph CreateLexDfa(params RegexExpr[] expressions)
        {
            return CreateLexDfa(expressions.AsEnumerable());
        }

        protected DfaGraph CreateLexDfa(IEnumerable<RegexExpr> expressions)
        {
            var nfa = LexNfaAutomata.BuildNfa(expressions);
            var nfaGraph = nfa.ToNfaGraph();

            var dfa = LexDfaAutomata.BuildDfa(nfaGraph);
            return dfa.ToDfaGraph();
        }
    }
}
