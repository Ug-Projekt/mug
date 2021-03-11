using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class EnumMemberNode : INode
    {
        public string NodeKind => "EnumMember";
        public string Name { get; set; }
        public Token Value { get; set; }
        public Range Position { get; set; }
    }
}
