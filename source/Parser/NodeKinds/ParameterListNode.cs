using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser.NodeKinds
{
    public struct Parameter
    {
        public readonly Token Type;
        public readonly String Name;
        public readonly Token DefaultConstantValue;
        public Parameter(Token type, string name, Token defaultConstValue)
        {
            Type = type;
            Name = name;
            DefaultConstantValue = defaultConstValue;
        }
        public string Stringize(string indent)
        {
            return indent+$"Type: {{\n{indent}   {Type}\n{indent}}},\n{indent}Name: {Name},\n{indent}DefaultConstantValue: {{\n{indent}   {DefaultConstantValue}\n{indent}}}";
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
        public void Add(Parameter parameter)
        {
            parameters.Add(parameter);
        }
        public string Stringize(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < parameters.Count; i++)
                nodes += indent+"   "+"Parameter["+i+"] {\n"+parameters[i].Stringize(indent+"      ")+'\n'+indent+"   },\n";
            return indent+$"ParameterListNode: {{\n{nodes}{indent}}}";
        }
    }
}