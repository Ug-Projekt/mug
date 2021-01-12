using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class AssignmentStatement : INode 
    {
        public TokenKind Operator { get; set; }
        public INode Left { get; set; }
        public INode Right { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+ $"AssignmentStatement: {{\n{indent}   Name: {{\n{Left.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Operator: {Operator},\n{indent}   Body: {{\n{(Right is not null ? Right.Stringize(indent+"      ") : "")}\n{indent}   }}\n{indent}}}";
        }
    }
}
