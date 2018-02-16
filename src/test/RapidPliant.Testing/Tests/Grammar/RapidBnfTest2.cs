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
    public class RapidBnfTest2 : TestBase
    {
        protected override void Test()
        {
            var input = new StringBuffer(rbnfGrammarInputText);
            var lexer = RapidBnfGrammar.CreateLexer();
            lexer.Init(input);
            
            var count = 10;
            var sw = new Stopwatch();
            
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

            for (var i = 0; i < count; ++i)
            {
                sw.Start();

                lexer.Init(input);
                tokens = new BufferTokenStream(new LexerTokenStream(lexer).ReadAllTokens());
                tokens.Reset();
                var context = new ParseContext(tokens);

                //TestAst(context);
                
                if (!RapidBnfGrammar.ParseGrammar(context.Start()))
                {
                    // error
                }
                else
                {
                    // success
                }

                sw.Stop();
            }

            Console.WriteLine($"{sw.ElapsedMilliseconds / count}");

            while (true)
            {
                sw.Restart();

                lexer.Init(input);
                tokens = new BufferTokenStream(new LexerTokenStream(lexer).ReadAllTokens());
                tokens.Reset();
                var context = new ParseContext(tokens);

                //TestAst(context);

                if (!RapidBnfGrammar.ParseGrammar(context.Start()))
                {
                    // error
                }
                else
                {
                    // success
                }

                sw.Stop();
                
                Console.WriteLine($"{sw.ElapsedMilliseconds}");

                if(Console.ReadLine() != "")
                    break;
            }

            
        }

        #region input 
        //        private static readonly string grammarInputText = @"
        //RuleA = 132.5345 RuleB | 'some' RuleC /a test | some/ /another (testing here)/;
        //RuleB = 'test';
        //RuleC = RuleA;
        //";

        private static readonly string grammarInputText = @"
//This is a line comment
/* this is a 
multi line 
cool pattern */
RuleA = (group one)?. | (group two)* /a test | some/ .4 /another (testing here)/;
";

        private static readonly string rbnfGrammarInputText = @"
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
	= 'import' id
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
	
RuleExpression
	= (
	| SpellingExpr
	| RegexExpr
	| GroupExpr
	| RefExpr
	) ExprOp? PinOp?
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
        private static readonly string bigInputText = @"
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
RuleA = RuleB | RuleC | RuleC | RuleD | RuleX;
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
