using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public struct Parameter
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
        public Parameter(INode type, string name, Token defaultConstValue, bool isSelf = false)
        {
            IsSelf = isSelf;
            Type = type;
            Name = name;
            DefaultConstantValue = defaultConstValue;
        }
        public string Stringize(string indent)
        {
            return indent+$"Type: {{\n{Type.Stringize(indent+"   ")}\n{indent}}},\n{indent}Name: {Name},\n{indent}IsSelf: {IsSelf},\n{indent}DefaultConstantValue: {{\n{DefaultConstantValue.Stringize(indent+"   ")}\n{indent}}}";
        }
    }
    public class ParameterListNode : INode
    {
        public Parameter[] Parameters
        {
            get
            {
                return parameters.ToArray();
            }
        }

        public Range Position { get; set; }

        List<Parameter> parameters = new();
        public bool HasInstanceParam
        {
            get
            {
                return parameters.Count > 0 && parameters[0].IsSelf;
            }
        }
        public void Add(Parameter parameter)
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