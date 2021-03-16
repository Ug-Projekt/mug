﻿using Mug.Models.Lexer;
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
        public bool IsReference { get; }
        public Range Position { get; set; }

        public ParameterNode(bool isreference, MugType type, string name, Token defaultConstValue, Range position)
        {
            IsReference = isreference;
            Type = type;
            Name = name;
            Position = position;
            DefaultConstantValue = defaultConstValue;
        }
    }
    public class ParameterListNode : INode
    {
        public string NodeKind => "ParameterList";
        public ParameterNode[] Parameters
        {
            get
            {
                return parameters.ToArray();
            }
        }

        public Range Position { get; set; }
        public int Lenght
        {
            get
            {
                return parameters.Count;
            }
        }

        private readonly List<ParameterNode> parameters = new();
        public void Add(ParameterNode parameter)
        {
            parameters.Add(parameter);
        }
    }
}