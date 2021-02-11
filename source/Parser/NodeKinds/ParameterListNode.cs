using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;

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

        private readonly List<ParameterNode> parameters = new();
        public void Add(ParameterNode parameter)
        {
            parameters.Add(parameter);
        }
    }
}