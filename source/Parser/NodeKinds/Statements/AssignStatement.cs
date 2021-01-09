using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct AssignStatement : IStatement 
    {
        public MemberAccessNode Name { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+ $"AssignStatement: {{\n{indent}   Name: {{\n{Name.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Body: {{\n{Body.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
