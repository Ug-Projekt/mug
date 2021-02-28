using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds.Directives
{
    public class UseDirective : INode
    {
        public string NodeKind => "UseDirective";
        public INode Body { get; set; }
        public Token Alias { get; set; }
        public Range Position { get; set; }
    }
}
