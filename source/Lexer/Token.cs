using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lexer
{
    public struct Token
    {
        public readonly int LineAt;
        public readonly TokenKind Kind;
        public readonly object Value;
        public readonly Range Position;
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
