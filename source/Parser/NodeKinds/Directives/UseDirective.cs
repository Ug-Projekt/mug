using Mug.Models.Lexer;
using System;

namespace Mug.Models.Parser.NodeKinds.Directives
{
    public enum UseMode
    {
        UsingAlias,
        UsingNamespace,
    }
    public class UseDirective : INode
    {
        public INode Body { get; set; }
        public Token Alias { get; set; }
        public UseMode Mode { get; set; }
        public Range Position { get; set; }
    }
}
