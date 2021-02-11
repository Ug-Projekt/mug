using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class LoopManagmentStatement : INode
    {
        public Token Managment { get; set; }
        public Range Position { get; set; }
    }
}
