using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lexer
{
    public struct Token
    {
        public readonly Int32 LineAt;
        public readonly TokenKind Kind;
        public readonly String Value;
        public readonly Range Position;
        public Token(int lineAt, TokenKind kind, string value, Range position)
        {
            LineAt = lineAt;
            Kind = kind;
            Value = value;
            Position = position;
        }
        public override string ToString() => $"Line({LineAt}, {Position.Start}:{Position.End}) {Kind}: '{(Value is null ? "<null>" : Value)}'";
    }
}
