using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class ReturnStatement : IStatement
    {
        public Boolean IsDefined { get; set; } = true;
        public INode Body { get; set; }
        public Range Position { get; set; }

        public string Stringize(string indent = "")
        {
            return indent+$"ReturnStatement: (({Position.Start}:{Position.End}) Body:\n{Body.Stringize(indent+"   ")})";
        }
    }
}
