using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct ConditionalStatement : IStatement 
    {
        public TokenKind Kind { get; set; }
        public INode Expression { get; set; }
        public BlockNode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent + $"ConditionalStatement: {{\n{indent}   Kind: {Kind},\n{indent}   Expression: {{\n{(Kind != TokenKind.KeyElse ? Expression.Stringize(indent+"      ") : "")}\n{indent}   }},\n{indent}   Body: {{\n{Body.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
