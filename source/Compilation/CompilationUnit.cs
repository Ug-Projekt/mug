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
        string ModuleName;
        IRGenerator IRGenerator;
        public CompilationUnit(string moduleName, string source)
        {
            ModuleName = moduleName;
            IRGenerator = new (source);
        }
        public CompilationUnit(string path)
        {
            if (!File.Exists(path))
                debug.exit("`", path, "`: ", "Invalid Path");
            ModuleName = path;
            IRGenerator = new (File.ReadAllText(path));
        }
        public List<Token> GetTokenCollection() => IRGenerator.GetTokenCollection();
    }
}
