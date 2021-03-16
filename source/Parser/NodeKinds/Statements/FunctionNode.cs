using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class FunctionNode : INode
    {
        public string NodeKind => "Function";
        public Pragmas Pragmas { get; set; }
        public string Name { get; set; }
        public MugType Type { get; set; }
        public ParameterListNode ParameterList { get; set; } = new();
        public List<Token> Generics { get; set; } = new();
        public BlockNode Body { get; set; } = new();
        public Range Position { get; set; }
        public TokenKind Modifier { get; set; }
        public ParameterNode? Base { get; set; }
    }
}
