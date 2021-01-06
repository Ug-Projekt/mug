using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public struct AssignStatement : IStatement 
    {
        public String Name { get; set; }
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+ "AssignStatement: " + $"(({Position.Start}:{Position.End}) Name: {Name}, Body:\n{Body.Stringize(indent+"   ")})";
        }
    }
}
