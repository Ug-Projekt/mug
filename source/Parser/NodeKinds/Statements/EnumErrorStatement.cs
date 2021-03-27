using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.MugValueSystem;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class EnumErrorStatement : INode
    {
        public string NodeKind => "EnumError";
        public string Name { get; set; }
        public List<Token> Body { get; set; } = new();
        public TokenKind Modifier { get; set; }
        public Range Position { get; set; }
    }
}
