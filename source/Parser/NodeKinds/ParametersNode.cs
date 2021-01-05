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
        public override string ToString()
        {
            return $"(Type: {Type}, Name: {Name}, DefaultConstantValue: {DefaultConstantValue})";
        }
    }
    public class ParametersNode : INode
    {
        public Parameter[] Parameters
        {
            get
            {
                return parameters.ToArray();
            }
        }
        List<Parameter> parameters = new();
        public void Add(Parameter parameter)
        {
            parameters.Add(parameter);
        }
        public string Stringize(string indent = "")
        {
            string nodes = "";
            for (int i = 0; i < parameters.Count; i++)
                nodes += indent+"   "+parameters[i].ToString()+'\n';
            return indent+"ParametersNode: "+(parameters.Count > 0 ? $"(Parameters:\n{string.Join(", ", parameters)})" : "(empty)");
        }
    }
}