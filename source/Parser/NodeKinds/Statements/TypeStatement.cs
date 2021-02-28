﻿using Mug.Models.Lexer;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser.NodeKinds.Statements
{
    public class TypeStatement : INode
    {
        public string NodeKind => "Struct";
        public String Name { get; set; }
        private List<Token> _genericTypes { get; set; } = new();
        public Token[] GenericTypes
        {
            get
            {
                return _genericTypes.ToArray();
            }
        }
        public Boolean IsGeneric
        {
            get
            {
                return GenericTypes.Length > 0;
            }
        }

        private List<FieldNode> _body { get; set; } = new();
        public FieldNode[] Body
        {
            get
            {
                return _body.ToArray();
            }
        }
        public Range Position { get; set; }
        public void AddGenericType(Token type)
        {
            _genericTypes.Add(type);
        }
        public void AddField(FieldNode field)
        {
            _body.Add(field);
        }
    }
}
