using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class AssignmentStatement : INode 
    {
        public TokenKind Operator { get; set; }
        public INode Name { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+ $"AssignmentStatement: {{\n{indent}   Name: {{\n{Name.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Operator: {Operator},\n{indent}   Body: {{\n{(Body is not null ? Body.Stringize(indent+"      ") : "")}\n{indent}   }}\n{indent}}}";
        }
    }
}
