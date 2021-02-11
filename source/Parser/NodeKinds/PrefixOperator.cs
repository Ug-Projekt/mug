﻿using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class PrefixOperator : INode
    {
        public INode Expression { get; set; }
        public TokenKind Prefix { get; set; }
        public Range Position { get; set; }
    }
}
