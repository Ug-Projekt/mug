using Mug.TypeSystem;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class CastExpressionNode : INode
    {
        public INode Expression { get; set; }
        public MugType Type { get; set; }
        public Range Position { get; set; }
    }
}