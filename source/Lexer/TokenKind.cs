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
        Dot
    }
}
