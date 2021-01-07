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
        public ParameterListNode ParameterList { get; set; }
        public Boolean IsMethod
        {
            get
            {
                return ParameterList.HasInstanceParam;
            }
        }
        public BlockNode Body { get; set; }
        public Range Position { get; set; }
        public string Stringize(string indent = "")
        {
            return indent+$"FunctionNode: {{\n{indent}   IsMethod: {IsMethod},\n{indent}   Type: {{\n{indent}      {Type}\n{indent}   }},\n{indent}   Name: {Name},\n{indent}   Modifier: {Modifier},\n{indent}   ParameterList: {{\n{ParameterList.Stringize(indent+"      ")}\n{indent}   }},\n{indent}   Body: {{\n{Body.Stringize(indent+"      ")}\n{indent}   }}\n{indent}}}";
        }
    }
}
