using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public struct ParameterNode : INode
    {
        public MugType Type { get; }
        public String Name { get; }
        public Token DefaultConstantValue { get; }
        public Boolean IsOptional
        {
            get
            {
                return DefaultConstantValue.Kind != TokenKind.Bad;
            }
        }

        public Range Position { get; set; }

        public ParameterNode(MugType type, string name, Token defaultConstValue, Range position)
        {
            Type = type;
            Name = name;
            Position = position;
            DefaultConstantValue = defaultConstValue;
        }
        public string Dump(string indent)
        {
            return indent+$"Type: {{\n{Type.Dump(indent+"   ")}\n{indent}}},\n{indent}Name: {Name},\n{indent}DefaultConstantValue: {{\n{DefaultConstantValue.Dump(indent+"   ")}\n{indent}}}";
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
        public void Add(ParameterNode parameter)
        {
            parameters.Add(parameter);
        }
        public string Dump(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < parameters.Count; i++)
                nodes += indent+"   Parameter["+i+"] {\n"+parameters[i].Dump(indent+"      ")+'\n'+indent+"   },\n";
            return indent+$"ParameterListNode: {{\n{nodes}{indent}}}";
        }
    }
}