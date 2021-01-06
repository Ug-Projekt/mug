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
            return indent + $"CallStatement: (({Position.Start}:{Position.End}) Name: {Name}, Parameters:{(HasParameters ? "\n"+Parameters.Stringize(indent+"   ") : "(empty)")})";
        }
    }
}
