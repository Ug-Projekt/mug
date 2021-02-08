﻿using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class BooleanExpressionNode : INode
    {
        public INode Left { get; set; }
        public INode Right { get; set; }
        public TokenKind Operator { get; set; }
        public Range Position { get; set; }
        public string Dump(string indent = "")
        {
            return indent+ $"BooleanExpressionNode: {{\n{indent}   Operator: {Operator},\n{indent}   Left: {{\n{Left.Dump(indent+"      ")}\n{indent}   }},\n{indent}   Right: {{\n{Right.Dump(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
