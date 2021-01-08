using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct VariableStatement : IStatement 
    {
        public String Name { get; set; }
        public Token Type { get; set; }
        public Boolean IsAssigned { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+$"VariableStatement: {{\n{indent}   Type: {{\n{indent}      {Type.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Name: {Name},\n{indent}   IsAssigned: {IsAssigned}{(IsAssigned ? $",\n{indent}   Body: {{\n{Body.Stringize(indent+"      ")}\n{indent}   }}" : "")}\n{indent}}}";
        }
    }
}
