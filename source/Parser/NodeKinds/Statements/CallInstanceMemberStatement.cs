using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class CallInstanceMemberStatement : IStatement
    {
        public INode Call { get; set; }
        public INode Instance { get; set; }
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            return indent+$"CallInstanceMemberStatement: {{\n{indent}   Instance: {{\n{Instance.Stringize(indent + "      ")}\n{indent}   }}\n{indent}   Call: {{\n{Call.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
        public override string ToString()
        {
            return Stringize();
        }
    }
}
