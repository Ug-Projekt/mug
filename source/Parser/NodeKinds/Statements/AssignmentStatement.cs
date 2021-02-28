using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class AssignmentStatement : INode
    {
        public string NodeKind => "Assignment";
        public TokenKind Operator { get; set; }
        public INode Name { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }
    }
}
