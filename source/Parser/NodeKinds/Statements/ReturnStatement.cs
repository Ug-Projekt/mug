using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class ReturnStatement : INode
    {
        public INode Body { get; set; }
        public Range Position { get; set; }
        public Boolean IsVoid
        {
            get
            {
                return Body is null;
            }
        }
        public string Dump(string indent = "")
        {
            return indent + $"ReturnStatement: {{\n{indent}   Body: {{\n{(Body is not null ? Body.Dump(indent + "      ") : "")}\n{indent}   }}\n{indent}}}";
        }
    }
}
