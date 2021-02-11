using Mug.Models.Parser;
using System;

namespace Mug.Models.Lexer
{
    public struct Token : INode
    {
        public TokenKind Kind { get; }
        public string Value { get; }
        public Range Position { get; set; }

        public Token(TokenKind kind, string value, Range position)
        {
            Kind = kind;
            Value = value;
            Position = position;
        }

        public override bool Equals(Object other)
        {
            if(!(other is Token)) return false;

            Token otherToken = (Token)other;

            if (!otherToken.Kind.Equals(this.Kind)) return false;
            if (!otherToken.Value.Equals(this.Value)) return false;
            if (!otherToken.Position.Equals(this.Position)) return false;

            return true;
        }
    }
}
