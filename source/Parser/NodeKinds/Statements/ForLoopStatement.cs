using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct ForLoopStatement : INode
    {
        public TokenKind Operator { get; set; }
        // VariableStatement, AssignStatement, ForCounterReference
        public INode Counter { get; set; }
        public INode RightExpression { get; set; }
        public BlockNode Body { get; set; }
        public Range Position { get; set; }
    }
}
