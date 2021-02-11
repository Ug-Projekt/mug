using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class MemberNode : INode
    {
        public INode Base { get; set; }
        public INode Member { get; set; }
        public Range Position { get; set; }
    }
}
