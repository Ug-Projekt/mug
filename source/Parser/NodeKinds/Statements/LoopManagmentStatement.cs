using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class LoopManagmentStatement : INode
    {
        public string NodeKind => "LoopManagment";
        public Token Managment { get; set; }
        public Range Position { get; set; }
    }
}
