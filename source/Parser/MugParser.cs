using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models
{
    public class MugParser
    {
        MugLexer Lexer;
        public MugParser(string moduleName, string source)
        {
            Lexer = new(moduleName, source);
        }
        public List<Token> GetTokenCollection() => Lexer.Tokenize();
    }
}
