using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class CallStatement : IStatement
    {
        public String Name { get; set; }
        public NodeBuilder Parameters { get; set; }
        public Boolean HasParameters
        {
            get
            {
                return Parameters != null;
            }
        }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent + $"CallStatement: {{\n{indent}   Name: {Name},\n{indent}   Parameters: {{\n{(HasParameters ? Parameters.Stringize(indent+"      ") : "")}\n{indent}   }}\n{indent}}}";
        }
    }
}
