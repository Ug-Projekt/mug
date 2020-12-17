using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mug.Models
{
    public class CompilationUnit
    {
        private string ModuleName;
        private MugLexer Lexer;
        public CompilationUnit(string moduleName, string userInput)
        {
            this.ModuleName = moduleName;
        }
        public CompilationUnit(string path)
        {
            if (!File.Exists(path))
                debug.exit("`", path, "`: ", "Invalid Path");
            this.ModuleName = path;
            this.Lexer = new (File.ReadAllText(path));
        }
    }
}
