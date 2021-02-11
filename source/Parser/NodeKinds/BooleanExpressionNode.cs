using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class BooleanExpressionNode : INode
    {
        public INode Left { get; set; }
        public INode Right { get; set; }
        public TokenKind Operator { get; set; }
        public Range Position { get; set; }
    }
}
