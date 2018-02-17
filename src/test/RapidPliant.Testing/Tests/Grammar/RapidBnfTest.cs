using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Lexer.Builder;
using RapidPliant.Lexing.Pattern.Regex;
using RapidPliant.Lexing.Text;
using RapidPliant.Parsing.Earley.HandRolled2;
using RapidPliant.Test;

namespace RapidPliant.Testing.Tests.Grammar
{
    public class RapidBnfTest : TestBase
    {
        protected override void Test()
        {
            ParseRepeated(RapidBnfInput.bigInputText, 100);

            ParseUntilUserInput(RapidBnfInput.bigInputText);
        }
        
        private ParseStats Parse(string inputText)
        {
            var input = new StringBuffer(inputText);
            var lexer = RapidBnfGrammar.CreateLexer();

            var stats = new ParseStats();

            Parse(lexer, input, stats);

            return stats;
        }

        private void ParseRepeated(string inputText, int repeatCount)
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

        private void ParseUntilUserInput(string inputText)
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

        private void PrintStats(ParseStats stats, int count = 1)
        {
            Console.WriteLine(stats.Print(count));
        }

        private ParseStats Parse(Lexer lexer, IBuffer input, ParseStats stats)
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

        private BufferTokenStream LexTokens(Lexer lexer, ParseStats stats)
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

        class ParseStats
        {
            private Stopwatch sw = new Stopwatch();

            public long LexTime { get; set; }
            public long ParseTime { get; set; }

            public long TotalTime => LexTime + ParseTime;

            public override string ToString()
            {
                return $"{TotalTime.ToString().PadLeft(3)} ({LexTime}/{ParseTime})";
            }

            public void Start()
            {
                sw.Restart();
            }

            public void Lexed()
            {
                sw.Stop();
                LexTime += sw.ElapsedMilliseconds;
                sw.Restart();
            }

            public void Parsed()
            {
                sw.Stop();
                ParseTime += sw.ElapsedMilliseconds;
                sw.Restart();
            }

            public void End()
            {
                sw.Stop();
            }

            public string Print(int count = 1)
            {
                return $"{(TotalTime / count).ToString().PadLeft(3)} ({(LexTime / count)}/{(ParseTime / count)})";
            }
        }
    }

    public static class RapidBnfInput
    {
        #region test input input
        //        private static readonly string grammarInputText = @"
        //RuleA = 132.5345 RuleB | 'some' RuleC /a test | some/ /another (testing here)/;
        //RuleB = 'test';
        //RuleC = RuleA;
        //";
        
        public static readonly string grammarInputText = @"
//This is a line comment
/* this is a 
multi line 
cool pattern */
RuleA = (group one)?. | (group two)* /a test | some/ .4 /another (testing here)/;
";
        #endregion

        #region rbnf grammar input
        public static readonly string rbnfGrammarInputText = @"
RuleExpression
	= (
	  SpellingExpr
	| RegexExpr
	| GroupExpr
	| RefExpr
	) ExprOp? PinOp?
	;

//RapidBnf grammar

//LEXING
id = /[a-zA-Z][a-zA-Z0-9_]*/;
spellingLiteral = /'[.^']'/;
regexLiteral = /'[.^']'/;


//PARSING
File 
	= TopDeclarations?
	;
	
TopDeclarations
	= TopDeclaration+
	;
	
TopDeclaration
	= ImportStatement
	| RuleStatement
	;
	
ImportStatement
	= 'import'.1 id
	;
	
RuleStatement
	= id '=' RuleDeclaration
	;
	
RuleDeclaration
	= RuleExpressions?
	;
	
RuleExpressions
	= (RuleExpression '|'?)+
	;
	
ExprOp
	= '*' | '?' | '+'
	;
	
PinOp
	= '.'
	;
	
SpellingExpr
	= spellingLiteral
	;
	
RegexExpr
	= regexLiteral
	;
	
GroupExpr
	= '(' RuleExpressions ')'
	;
	
RefExpr
	= id
	;
";
        #endregion

        #region big input
        /*
         * RuleA = RuleB / RuleY / RuleZ RuleI +++ RuleJ;
        RuleA = RuleB | RuleC | RuleC | RuleD ++ | RuleX;
         */
        public static readonly string bigInputText = @"
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
";

        #endregion
    }
}
