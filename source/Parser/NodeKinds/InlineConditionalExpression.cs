using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class InlineConditionalExpression : INode
    {
        public string NodeKind => "Ternary";
        public INode Expression { get; set; }
        public INode IFBody { get; set; }
        public INode ElseBody { get; set; }
        public Range Position { get; set; }
    }
}
