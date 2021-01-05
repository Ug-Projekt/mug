using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models
{
    public class IRGenerator
    {
        MugParser Parser;
        public IRGenerator(string moduleName, string source)
        {
            Parser = new (moduleName, source);
        }
        public List<Token> GetTokenCollection() => Parser.GetTokenCollection();
        public List<Token> GetTokenCollection(out MugLexer lexer) => Parser.GetTokenCollection(out lexer);
    }
}
