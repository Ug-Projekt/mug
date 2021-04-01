﻿using Mug.Models.Lexer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Mug.Models.Parser.NodeKinds
{
    public class PrefixOperator : IStatement
    {
        public string NodeKind => "PrefixOperator";
        public INode Expression { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TokenKind Prefix { get; set; }
        public Range Position { get; set; }
    }
}
