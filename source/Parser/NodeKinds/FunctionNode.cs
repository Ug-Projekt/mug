using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public enum Modifier
    {
        Private,
        Public,
        Instance
    }
    public class FunctionNode : INode
    {
        public String Name { get; set; }
        public Token Type { get; set; }
        public Modifier Modifier { get; set; }
        public ParametersNode Parameters { get; set; }
        public BlockNode Body { get; set; }
        // Name Position
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            return indent+"FunctionNode: "+$"(({Position.Start}:{Position.End}) Type:\n{indent+Type},\n{indent}Name: {Name}, Modifier: {Modifier}, Parameters:\n{Parameters.Stringize(indent+"   ")}, Body:\n{Body.Stringize(indent+"   ")});";
        }
    }
}
