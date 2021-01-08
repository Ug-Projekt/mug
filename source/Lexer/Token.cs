using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lexer
{
    public struct Token
    {
        public Int32 LineAt { get; }
        public TokenKind Kind { get; }
        public Object Value { get; }
        public Range Position { get; }
        public Token(int lineAt, TokenKind kind, object value, Range position)
        {
            LineAt = lineAt;
            Kind = kind;
            Value = value;
            Position = position;
        }
        public string Stringize(string indent = "")
        {
            return Value is MemberAccessNode ? $"{{\n{indent}{Kind}: {{\n{((MemberAccessNode)Value).Stringize(indent+"   ")}\n{indent}}}\n{indent[..^3]}}}" : ToString();
        }
        public override string ToString() => $"{Kind}: '{(Value is null ? "<null>" : Value)}'";
    }
}
