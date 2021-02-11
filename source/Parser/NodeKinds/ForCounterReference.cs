using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class ForCounterReference : INode
    {
        public INode ReferenceName { get; set; }
        public Range Position { get; set; }
    }
}
