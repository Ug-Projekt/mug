using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class ExpressionNode : INode
    {
        public INode Left { get; set; }
        public INode Right { get; set; }
        public OperatorKind Operator { get; set; }
        public Range Position { get; set; }
    }
}
