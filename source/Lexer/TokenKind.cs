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
        EOF
    }
}
