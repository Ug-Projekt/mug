using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class InParExpressionNode : INode
    {
        public INode Content { get; set; }
        public Range Position { get; set; }
    }
}
