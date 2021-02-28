using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class ForCounterReference : INode
    {
        public string NodeKind => "ForCounterReference";
        public INode ReferenceName { get; set; }
        public Range Position { get; set; }
    }
}
