using System;
using System.Collections.Generic;
using System.Text;
using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds;

namespace Mug.Models.Parser.NodeKinds.Directives
{
    public enum UseMode
    {
        RedefiningAlias,
        UsingNamespace,
    }
    public class UseDirective : IDirective
    {
        public MemberAccessNode Member { get; set; }
        public Token Alias { get; set; }
        public UseMode Mode { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent + $"UseDirective: {{\n{indent}   Mode: {Mode},\n{indent}   Alias: {Alias},\n{indent}   Member: {{\n{Member.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
