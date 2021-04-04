﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class ExpressionNode : INode
    {
        public string NodeKind => "BinaryExpression";
        public INode Left { get; set; }
        public INode Right { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public OperatorKind Operator { get; set; }
        public Range Position { get; set; }
    }
}
