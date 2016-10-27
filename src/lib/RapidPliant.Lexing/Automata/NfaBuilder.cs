using System;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Lexing.Pattern;

namespace RapidPliant.Lexing.Automata
{
    public class NfaBuildContext : IDisposable
    {
        public NfaBuildContext()
        {
        }

        #region Operation helpers
        public Nfa Or(params Nfa[] nfas)
        {
            return Or(nfas.AsEnumerable());
        }

        public Nfa Or(IEnumerable<Nfa> nfas)
        {
            var start = new NfaState();
            var end = new NfaState();

            foreach (var nfa in nfas)
            {
                start.AddTransistion(new NullNfaTransition(nfa.Start).WithExpression(nfa.Expression));
                nfa.End.AddTransistion(new NullNfaTransition(end).WithExpression(nfa.Expression));
            }
            
            return new Nfa(start, end);
        }

        public Nfa And(params Nfa[] nfas)
        {
            return And(nfas.AsEnumerable());
        }

        public Nfa And(IEnumerable<Nfa> nfas)
        {
            IExpr startNfaExpression = null;
            NfaState start = null;
            NfaState end = null;
            
            Nfa prevNfa = null;
            foreach (var nfa in nfas)
            {
                if (start == null)
                {
                    startNfaExpression = nfa.Expression;
                    start = nfa.Start;
                }

                if (prevNfa != null)
                {
                    prevNfa.End.AddTransistion(new NullNfaTransition(nfa.Start).WithExpression(nfa.Expression));
                }

                end = nfa.End;

                prevNfa = nfa;
            }

            return new Nfa(start, end).WithExpression(startNfaExpression);
        }

        public Nfa ZeroOrMore(Nfa nfa)
        {
            var start = new NfaState();
            var nullToNfaStart = new NullNfaTransition(nfa.Start).WithExpression(nfa.Expression);

            start.AddTransistion(nullToNfaStart);
            nfa.End.AddTransistion(nullToNfaStart);

            var end = new NfaState();
            var nullToNewEnd = new NullNfaTransition(end).WithExpression(nfa.Expression);

            start.AddTransistion(nullToNewEnd);
            nfa.End.AddTransistion(nullToNewEnd);

            return new Nfa(start, end).WithExpression(nfa.Expression);
        }

        public Nfa OneOrMore(Nfa nfa)
        {
            var end = new NfaState();
            nfa.End.AddTransistion(new NullNfaTransition(end).WithExpression(nfa.Expression));
            nfa.End.AddTransistion(new NullNfaTransition(nfa.Start).WithExpression(nfa.Expression));
            return new Nfa(nfa.Start, end).WithExpression(nfa.Expression);
        }

        public Nfa Optional(Nfa nfa)
        {
            var start = new NfaState();
            var end = new NfaState();
            start.AddTransistion(new NullNfaTransition(nfa.Start).WithExpression(nfa.Expression));
            start.AddTransistion(new NullNfaTransition(end).WithExpression(nfa.Expression));
            nfa.End.AddTransistion(new NullNfaTransition(end).WithExpression(nfa.Expression));
            return new Nfa(start, end).WithExpression(nfa.Expression);
        }
        #endregion

        #region Factory helpers
        public Nfa NfaForEmpty(IExpr expr)
        {
            var start = new NfaState();
            var end = new NfaState();
            start.AddTransistion(new NullNfaTransition(end).WithExpression(expr));
            return new Nfa(start, end).WithExpression(expr);
        }

        public Nfa NfaForAnyChar(IExpr expr)
        {
            var start = new NfaState();
            var end = new NfaState();
            var terminal = new AnyTerminal();
            var transition = new TerminalNfaTransition(terminal, end).WithExpression(expr);
            start.AddTransistion(transition);
            return new Nfa(start, end).WithExpression(expr);
        }

        public Nfa NfaForChar(char character, IExpr expr)
        {
            var start = new NfaState();
            var end = new NfaState();
            var terminal = new CharacterTerminal(character);
            var transition = new TerminalNfaTransition(terminal, end).WithExpression(expr);
            start.AddTransistion(transition);
            return new Nfa(start, end).WithExpression(expr);
        }

        public Nfa NfaForCharRange(char startChar, char endChar, IExpr expr, bool negate = false)
        {
            var start = new NfaState();
            var end = new NfaState();
            BaseTerminal terminal = new CharacterRangeTerminal(startChar, endChar);
            if (negate)
                terminal = new NegationTerminal(terminal);
            start.AddTransistion(new TerminalNfaTransition(terminal, end).WithExpression(expr));
            return new Nfa(start, end).WithExpression(expr);
        }

        public Nfa NfaForCharClass(string charClass, IExpr expr, bool negate = false)
        {
            var start = new NfaState();
            var end = new NfaState();
            var terminal = CreateCharClassTerminal(charClass, negate);
            var transition = new TerminalNfaTransition(terminal, end).WithExpression(expr);
            start.AddTransistion(transition);
            return new Nfa(start, end).WithExpression(expr);
        }

        private BaseTerminal CreateCharClassTerminal(string charClass, bool negate)
        {
            BaseTerminal terminal = null;
            switch (charClass)
            {
                case "s":
                    terminal = new WhitespaceTerminal();
                    break;
                case "d":
                    terminal = new DigitTerminal();
                    break;
                case "w":
                    terminal = new WordTerminal();
                    break;
                case "D":
                    terminal = new DigitTerminal();
                    negate = false;
                    break;
                case "S":
                    terminal = new WhitespaceTerminal();
                    negate = false;
                    break;
                case "W":
                    terminal = new WordTerminal();
                    negate = false;
                    break;
            }

            if(terminal == null) 
                throw new Exception($"Unhandled character class '{charClass}'!");

            if(negate)
                terminal = new NegationTerminal(terminal);

            return terminal;
        }
        #endregion

        public void Dispose()
        {
        }
    }

    public class NfaBuilder
    {
        public NfaBuilder()
        {
        }

        public Nfa Create(IEnumerable<IExpr> expressions)
        {
            using (var buildContext = new NfaBuildContext())
            {
                return BuildForAlterationExpression(buildContext, expressions);
            }
        }

        public Nfa Create(IExpr expr)
        {
            using (var buildContext = new NfaBuildContext())
            {
                return BuildForExpression(buildContext, expr).WithExpression(expr);
            }
        }

        private Nfa BuildForExpression(NfaBuildContext c, IExpr expr)
        {
            var isLeaf = !expr.IsGroup;

            Nfa exprNfa;

            if (isLeaf)
            {
                exprNfa = BuildForLeafExpression(c, expr).WithExpression(expr);
            }
            else
            {
                exprNfa = BuildForGroupExpression(c, expr).WithExpression(expr);
            }

            if (expr.HasOptions)
            {
                var exprOpt = expr.Options;
                if (exprOpt.IsMany)
                {
                    if (exprOpt.IsOptional)
                    {
                        exprNfa = c.ZeroOrMore(exprNfa).WithExpression(expr);
                    }
                    else
                    {
                        exprNfa = c.OneOrMore(exprNfa).WithExpression(expr);
                    }
                }
                else if (exprOpt.IsOptional)
                {
                    exprNfa = c.Optional(exprNfa).WithExpression(expr);
                }
            }

            return exprNfa;
        }
        
        private Nfa BuildForLeafExpression(NfaBuildContext c, IExpr expr)
        {
            var termExpr = expr as IPatternTerminalCharExpr;
            if (termExpr != null)
            {
                return c.NfaForChar(termExpr.Char, expr);
            }

            var termRangeExpr = expr as IPatternTerminalCharRangeExpr;
            if (termRangeExpr != null)
            {
                return c.NfaForCharRange(termRangeExpr.FromChar, termRangeExpr.ToChar, expr);
            }

            throw new Exception($"Unhandled leaf expression '{expr.ToString()}' of type '{expr.GetType().Name}'!");
        }

        private Nfa BuildForGroupExpression(NfaBuildContext c, IExpr expr)
        {
            var isProduction = expr.IsProduction;
            var isAlteration = expr.IsAlteration;

            if (isProduction)
            {
                return BuildForProductionExpression(c, expr).WithExpression(expr);
            }
            else if (isAlteration)
            {
                return BuildForAlterationExpression(c, expr).WithExpression(expr);
            }
            else
            {
                throw new Exception($"Invalid group expression '{expr.ToString()}' - should be either Production or Alteration!");
            }
        }

        private Nfa BuildForProductionExpression(NfaBuildContext c, IExpr expr)
        {
            return BuildForProductionExpression(c, expr.Expressions).WithExpression(expr);
        }

        private Nfa BuildForProductionExpression(NfaBuildContext c, IEnumerable<IExpr> expressions)
        {
            var nfas = new List<Nfa>();

            foreach (var expr in expressions)
            {
                var nfa = BuildForExpression(c, expr).WithExpression(expr);
                nfas.Add(nfa);
            }

            return c.And(nfas);
        }

        private Nfa BuildForAlterationExpression(NfaBuildContext c, IExpr expr)
        {
            return BuildForAlterationExpression(c, expr.Expressions).WithExpression(expr);
        }

        private Nfa BuildForAlterationExpression(NfaBuildContext c, IEnumerable<IExpr> expressions)
        {
            var nfas = new List<Nfa>();

            foreach (var expr in expressions)
            {
                var nfa = BuildForExpression(c, expr).WithExpression(expr);
                nfas.Add(nfa);
            }

            return c.Or(nfas);
        }
    }

    public class LexNfaBuilder : NfaBuilder
    {
        //Extends the base nfa builder with specifics for lex nfa
    }
}
