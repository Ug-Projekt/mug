﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lexer
{
    public enum TokenKind
    {
        Bad,
        Identifier,
        ConstantString,
        ConstantDigit,
        EOF,
        ConstantChar,
        KeyFunc,
        KeyVar,
        KeyConst,
        OpenPar,
        ClosePar,
        Semicolon,
        KeyTVoid,
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
        KeyTunknown,
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
        Star,
        KeyTarray,
        KeyReturn,
        Block,
        ConstantBoolean,
        KeyIF,
        KeyELIF,
        KeyELSE,
        BoolOperatorMinor,
        BoolOperatorMajor,
        BoolOperatorMinEQ,
        BoolOperatorMajEQ
    }
}
