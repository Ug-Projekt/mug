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
        public override string ToString() => $"Line({LineAt}, {Position.Start}:{Position.End}) {Kind}: '{(Value is null ? "<null>" : Value)}'";
    }
}
