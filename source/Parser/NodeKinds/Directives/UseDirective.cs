using System;
using System.Collections.Generic;
using System.Text;
using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds;

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

        public string Dump(string indent = "")
        {
            return indent + $"UseDirective: {{\n{indent}   Mode: {Mode},\n{indent}   Alias: {{\n{Alias.Dump(indent+"      ")}\n{indent}   }},\n{indent}   Member: {{\n{Body.Dump(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
