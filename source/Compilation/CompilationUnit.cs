using Mug.Models;
using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mug
{
    public class CompilationUnit
    {
        IRGenerator IRGenerator;
        public CompilationUnit(string moduleName, string source)
        {
            IRGenerator = new (moduleName, source);
        }
        public CompilationUnit(string path)
        {
            if (!File.Exists(path))
                debug.exit("`", path, "`: ", "Invalid Path");
            IRGenerator = new (path, File.ReadAllText(path));
        }
        public List<Token> GetTokenCollection() => IRGenerator.GetTokenCollection();
        public List<Token> GetTokenCollection(out MugLexer lexer) => IRGenerator.GetTokenCollection(out lexer);
    }
}
