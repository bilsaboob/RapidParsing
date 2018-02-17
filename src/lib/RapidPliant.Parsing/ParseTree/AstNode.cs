using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Lexer;

namespace RapidPliant.Parsing.ParseTree
{
    public interface IParseNode
    {
        IParseNode Parent { get; }

        IReadOnlyList<IParseNode> Children { get; }

        object Type { get; }
    }

    public class ParseNode : IParseNode
    {
        public IParseNode Parent { get; set; }

        public IReadOnlyList<IParseNode> Children { get; set; }

        public object Type { get; set; }
    }

    public interface ITerminalParseNode : IParseNode
    {
        IToken Token { get; }
    }

    public class AstNode
    {
        private IParseNode ParseNode { get; set; }

        public AstNode()
        {
        }

        public ITerminalParseNode GetTerminal(int index, TokenType tt)
        {
            var c = ParseNode.Children[index];
            if (c.Type == tt)
            {
                return (ITerminalParseNode) c;
            }
            return null;
        }

        public TNode GetNode<TNode>(int index)
            where TNode : AstNode
        {
            return ParseNode.Children[index] as TNode;
        }

        public IReadOnlyList<TNode> GetNodes<TNode>(int index)
            where TNode : AstNode
        {
            var children = new List<TNode>();

            for(var i = index; i < ParseNode.Children.Count; ++i)
            {
                var child = ParseNode.Children[i] as TNode;
                if (child != null)
                {
                    children.Add(child);
                }
            }
            
            return children;
        }
    }
}
