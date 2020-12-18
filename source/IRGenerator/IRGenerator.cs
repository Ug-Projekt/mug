using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models
{
    public class IRGenerator
    {
        MugParser Parser;
        public IRGenerator(string source)
        {
            Parser = new (source);
        }
        public List<Token> GetTokenCollection() => Parser.GetTokenCollection();
    }
}
