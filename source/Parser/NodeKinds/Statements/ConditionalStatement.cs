using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct ConditionalStatement : INode 
    {
        public TokenKind Kind { get; set; }
        public INode Expression { get; set; }
        public BlockNode Body { get; set; }
        public Range Position { get; set; }
    }
}
