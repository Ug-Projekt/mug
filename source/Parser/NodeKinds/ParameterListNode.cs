using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public struct ParameterNode : INode
    {
        public INode Type { get; }
        public String Name { get; }
        public Boolean IsSelf { get; }
        public Token DefaultConstantValue { get; }
        public Boolean IsOptional
        {
            get
            {
                return DefaultConstantValue.Kind != TokenKind.Bad;
            }
        }

        public Range Position { get; set; }

        public ParameterNode(INode type, string name, Token defaultConstValue, bool isSelf = false)
        {
            IsSelf = isSelf;
            Type = type;
            Name = name;
            Position = new();
            DefaultConstantValue = defaultConstValue;
        }
        public string Stringize(string indent)
        {
            return indent+$"Type: {{\n{Type.Stringize(indent+"   ")}\n{indent}}},\n{indent}Name: {Name},\n{indent}IsSelf: {IsSelf},\n{indent}DefaultConstantValue: {{\n{DefaultConstantValue.Stringize(indent+"   ")}\n{indent}}}";
        }
    }
    public class ParameterListNode : INode
    {
        public ParameterNode[] Parameters
        {
            get
            {
                return parameters.ToArray();
            }
        }

        public Range Position { get; set; }

        List<ParameterNode> parameters = new();
        public bool HasInstanceParam
        {
            get
            {
                return parameters.Count > 0 && parameters[0].IsSelf;
            }
        }
        public void Add(ParameterNode parameter)
        {
            parameters.Add(parameter);
        }
        public string Stringize(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < parameters.Count; i++)
                nodes += indent+"   Parameter["+i+"] {\n"+parameters[i].Stringize(indent+"      ")+'\n'+indent+"   },\n";
            return indent+$"ParameterListNode: {{\n{nodes}{indent}}}";
        }
    }
}