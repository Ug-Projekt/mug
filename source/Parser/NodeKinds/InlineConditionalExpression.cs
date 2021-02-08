using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public class InlineConditionalExpression : INode
    {
        public INode Expression { get; set; }
        public INode IFBody { get; set; }
        public INode ElseBody { get; set; }
        public Range Position { get; set; }
        public string Dump(string indent = "")
        {
            return indent + $"InlineConditionalExpression: {{\n{indent}   Expression: {{\n{Expression.Dump(indent+"      ")}\n{indent}   }},\n{indent}   IfBody: {{\n{IFBody.Dump(indent + "      ")}\n{indent}   }},\n{indent}   ElseBody: {{\n{ElseBody.Dump(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
