using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lexer
{
    public enum TokenKind
    {
        Identifier,
        ConstantString,
        ConstantDigit,
        EOF,
        Unknow,
        ConstantChar,
        KeyFunc,
        KeyVar,
        KeyConst,
        OpenPar,
        ClosePar,
        Semicolon,
        KeyVoid,
        Colon,
        OpenBrace,
        CloseBrace,
        OpenBracket,
        CloseBracket,
        ConstantFloatDigit,
        Dot,
        Comma,
        BoolOperatorEQ,
        Equal,
        Slash,
        BoolOperatorNEQ,
        KeySelf,
        KeyTi32,
        KeyTi8,
        KeyTi64,
        KeyTunknow,
        KeyTchr,
        KeyTstr,
        KeyTbool,
        KeyTu8,
        KeyTu32,
        KeyTu64,
        Increment,
        Decrement,
        IncrementAssign,
        DecrementAssign,
        Plus,
        Minus,
        Star
    }
}
