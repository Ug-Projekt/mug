using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class FieldAssignNode : INode
    {
        public String Name { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+ $"FieldAssignNode: {{\n{indent}   Name: {Name},\n{indent}   Body: {{\n{Body.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
