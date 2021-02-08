using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class FieldNode : INode
    {
        public String Name { get; set; }
        public MugType Type { get; set; }
        public Modifier Modifier { get; set; }
        public Range Position { get; set; }

        public string Dump(string indent = "")
        {
            return indent+$"FieldNode: {{\n{indent}   Name: {Name},\n{indent}   Modifier: {Modifier},\n{indent}   Type: {{\n{Type.Dump(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
