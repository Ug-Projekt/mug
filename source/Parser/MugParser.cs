using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models
{
    public class MugParser
    {
        MugLexer Lexer;
        public MugParser(string source)
        {
            Lexer = new(source);
        }
        public List<Token> GetTokenCollection() => Lexer.Tokenize();
    }
}
