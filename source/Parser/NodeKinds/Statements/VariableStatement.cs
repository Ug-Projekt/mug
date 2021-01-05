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
        public Boolean IsDefined { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+"VariableStatement: "+$"(({Position.Start}:{Position.End}) Type:\n{indent+Type},\n{indent}Name: {Name}, IsDefined: {IsDefined}{(IsDefined ? $", Body:\n{Body.Stringize(indent+"   ")})" : ")")}";
        }
    }
}
