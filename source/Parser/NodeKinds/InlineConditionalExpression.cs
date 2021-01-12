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
        public string Stringize(string indent = "")
        {
            return indent + $"InlineConditionalExpression: {{\n{indent}   Expression: {{\n{Expression.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   IfBody: {{\n{IFBody.Stringize(indent + "      ")}\n{indent}   }},\n{indent}   ElseBody: {{\n{ElseBody.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
