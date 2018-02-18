using System.Linq;
using RapidPliant.Grammar;
using RapidPliant.Grammar.Definitions;
using RapidPliant.Parsing.Automata.Dfa;
using RapidPliant.Parsing.Automata.Nfa;
using RapidPliant.Test;

namespace RapidPliant.RapidBnf.Test.Tests.Grammar
{
    public class SimpleGrammarTest : TestBase
    {
        protected override void Test()
        {
            TestSimpleGrammar();
        }

        private void TestSimpleGrammar()
        {
            var g = new SimpleGrammarModel();
            g.Build();
            var startRule = g.GetStartRules().FirstOrDefault();

            var nfa = EarleyNfaAutomata.BuildNfa(startRule.Expression);
            var nfaGraph = nfa.ToNfaGraph();
            var dfa = EarleyDfaAutomata.BuildDfa(nfaGraph);
        }
    }

    public class SimpleGrammarModel : SimpleGrammarModel<SimpleGrammarModel>
    {
        public RuleDef
            A = "A",
            B = "B",
            C = "C"
            ;

        public LexDef
            a = "a",
            b = "b",
            c = "c",
            d = "d",
            e = "e",
            f = "f",
            g = "g",
            k = "k"
            ;

        protected override void Define()
        {
            //Configure the lex declarations
            a.As(LexPattern("a"));
            b.As(LexPattern("b"));
            c.As(LexPattern("c"));
            d.As(LexPattern("d"));
            e.As(LexPattern("e"));
            f.As(LexPattern("f"));
            g.As(LexPattern("g"));
            k.As(LexPattern("k"));

            //Configure the rule declarations

            /*
            //Simple reference... nothing strange about this... no recursion or anything...
            A.As(
                a + B
            );
            B.As(
                c + d
            );
            */

            /*
            //Simple reference... nothing strange about this... no recursion or anything...
            A.As(
                B + a 
            );
            B.As(
                c + d
            );
            */

            
            //Simple reference... nothing strange about this... no recursion or anything...
            A.As(
                a + B + e
            );

            B.As(
                b + c + d
            );
            

            /*
            //Simple reference... nothing strange about this... no recursion or anything...
            A.As(
                B + e + f
            );
            B.As(
                c + d
            );
            */

            /*
            //Simple reference... nothing strange about this... no recursion or anything...
            A.As(
                B
            );
            B.As(
                c + d
            );
            */

            /*
            //Simple reference... nothing strange about this... no recursion or anything...
            A.As(
                a + B + e
            );

            B.As(
                b + c + d
            );
            */

            /*
            //Unresolvable recursion, causing infinite recursion... DFA will get stuck on a loob between ->a->b->a->b->a->etc...
            A.As(
                a + B + c
            );
            B.As(
                b + A + d
            );
            */


            /*
            //Directly resolvable, since A has a reachable end "a + k" when recursing from B
            A.As(
                a + B + c | k
            );

            B.As(
                b + A + d
            );
            */

            /*
            //Indirectly resolvable, since B has recursion to A, but still has one reachable end without "recursion" => k... so it's fine
            A.As(
                a + B + c
            );

            B.As(
                b + A + d | k
            );
            */

            /*
            //Directly resolvable between B and C, since B has reachable end "g"
            A.As(
                a + B + c | a + k
            );

            B.As(
                b + C + d | g
            );

            C.As(
                e + B + g
            );
            */

            Start(A);
        }
    }
}
