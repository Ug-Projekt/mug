using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class PostfixOperator : IStatement
    {
        public string NodeKind => "PostfixOperator";
        public INode Expression { get; set; }
        public TokenKind Postfix { get; set; }
        public Range Position { get; set; }
    }
}
