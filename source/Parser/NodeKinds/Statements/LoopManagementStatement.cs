using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class LoopManagementStatement : INode
    {
        public string NodeKind => "LoopManagement";
        public Token Management { get; set; }
        public Range Position { get; set; }
    }
}
