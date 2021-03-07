using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class MemberNode : INode
    {
        public string NodeKind => "Member";
        public INode Base { get; set; }
        public Token Member { get; set; }
        public Range Position { get; set; }
    }
}
