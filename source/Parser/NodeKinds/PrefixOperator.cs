using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class PrefixOperator : IStatement
    {
        public string NodeKind => "PrefixOperator";
        public INode Expression { get; set; }
        public TokenKind Prefix { get; set; }
        public Range Position { get; set; }
    }
}
