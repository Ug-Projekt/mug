using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct VariableStatement : INode
    {
        public String Name { get; set; }
        public INode Type { get; set; }
        public Boolean IsAssigned { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+$"VariableStatement: {{\n{indent}   Type: {{\n{Type.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Name: {Name},\n{indent}   IsAssigned: {IsAssigned}{(IsAssigned ? $",\n{indent}   Body: {{\n{Body.Stringize(indent+"      ")}\n{indent}   }}" : "")}\n{indent}}}";
        }
    }
}
