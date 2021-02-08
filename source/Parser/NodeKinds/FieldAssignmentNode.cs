using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class FieldAssignmentNode : INode
    {
        public String Name { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Dump(string indent = "")
        {
            return indent+ $"FieldAssignmentNode: {{\n{indent}   Name: {Name},\n{indent}   Body: {{\n{Body.Dump(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
