﻿using System;

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
