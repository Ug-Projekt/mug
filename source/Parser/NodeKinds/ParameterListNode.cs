using Mug.Models.Lexer;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds
{
    public struct ParameterNode : INode
    {
        public string NodeKind => "Parameter";
        public MugType Type { get; }
        public string Name { get; }
        public Token DefaultConstantValue { get; }
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
        public string NodeKind => "ParameterList";
        public Range Position { get; set; }
        public int Lenght
        {
            get
            {
                return Parameters.Count;
            }
        }

        public readonly List<ParameterNode> Parameters = new();
    }
}