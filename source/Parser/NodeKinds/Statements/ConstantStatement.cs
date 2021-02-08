using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct ConstantStatement : INode 
    {
        public String Name { get; set; }
        public MugType Type { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Dump(string indent = "")
        {
            return indent+$"ConstantStatement: {{\n{indent}   Type: {{\n{Type.Dump(indent+"      ")}\n{indent}   }},\n{indent}   Name: {Name},\n{indent}   Body: {{\n{Body.Dump(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
