﻿using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class ExpressionNode : INode
    {
        public INode Left { get; set; }
        public INode Rigth { get; set; }
        public OperatorKind Operator { get; set; }
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            return indent+"ExpressionNode: "+$"(({Position.Start}:{Position.End}) Operator: {Operator},\n{indent}   Left:\n{Left.Stringize(indent+"      ")},\n{indent}   Rigth:\n{Rigth.Stringize(indent+"       ")})";
        }
    }
}