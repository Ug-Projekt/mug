using Mug.Compilation;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lexer
{
    public struct Token : INode
    {
        public Int32 LineAt { get; }
        public TokenKind Kind { get; }
        public Object Value { get; }
        public Range Position { get; set; }

        public Token(int lineAt, TokenKind kind, object value, Range position)
        {
            LineAt = lineAt;
            Kind = kind;
            Value = value;
            Position = position;
        }
        public string Stringize(string indent = "")
        {
            return indent+$"Literal: {{\n{indent}   Kind: {Kind},\n{indent}   Value: {(Value is INode node ? $"{{\n{node.Stringize(indent+"      ")}\n{indent}   }}" : Value)}\n{indent}}}";
        }
        public override string ToString()
        {
            if (Kind == TokenKind.KeyTi32)
                return "i32";
            else if (Kind == TokenKind.KeyTVoid)
                return "?";
            return Value is null ? "" : Value.ToString();
        }
    }
}
