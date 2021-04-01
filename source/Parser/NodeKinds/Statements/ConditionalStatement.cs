using Mug.Models.Lexer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class ConditionalStatement : INode
    {
        public string NodeKind => "Condition";
        [JsonConverter(typeof(StringEnumConverter))]
        public TokenKind Kind { get; set; }
        public INode Expression { get; set; }
        public BlockNode Body { get; set; }
        public Range Position { get; set; }
        public ConditionalStatement ElseNode { get; set; }
    }
}
