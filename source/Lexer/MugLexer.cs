using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lexer
{
    public class MugLexer
    {
        public  string Source;
        public List<Token> TokenCollection;
        public MugLexer(string source)
        {
            CompilationErrors.Lexer = this;
            Source = source;
        }
        public List<Token> Tokenize()
        {
            if (TokenCollection is not null)
                return TokenCollection;
            TokenCollection = new ();

            return TokenCollection;
        }
    }
}
