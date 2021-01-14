using Mug.Models;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mug.Compilation
{
    public class CompilationUnit
    {
        public IRGenerator IRGenerator;
        public static String ModuleName { get; set; }
        static String temp => $"C:/Users/{Environment.UserName}/AppData/Local/Temp/mug/";
        static String tempbc => ModuleName + ".ll";
        static String tempexe => ModuleName + ".exe";
        String CompletePath;
        public CompilationUnit(string moduleName, string source)
        {
            ModuleName = moduleName;
            IRGenerator = new (moduleName, source);
        }
        public CompilationUnit(string path)
        {
            CompletePath = path;
            IRGenerator = new (Path.GetFileName(path), File.ReadAllText(path));
        }
        public string Compile()
        {
            IRGenerator.Parser.Lexer.Tokenize();
            IRGenerator.Parser.Parse();
            return IRGenerator.Generate();
        }
        public void Compile(string filePathDestination = null)
        {
            if (filePathDestination is null) {
                if (CompletePath is null)
                    CompilationErrors.Throw("Extern User: the user must pass a valid path");
                filePathDestination = CompletePath;
            }
            File.Delete(tempbc);
            File.Delete(tempexe);
            File.Delete(filePathDestination);
            var gen = Compile();
            File.WriteAllText(tempbc, gen);
            while (!File.Exists(tempbc));
            var clangCall = Process.Start("clang", tempbc + " -o " + tempexe);
            clangCall.WaitForExit();
            if (clangCall.ExitCode != 0)
                CompilationErrors.Throw("Extern Compiler: impossible to build due to prevoius errors");
            while (!File.Exists(tempexe));
            File.Move(tempexe, filePathDestination);
        }
        public string GetStringAST()
        {
            return GetNodeCollection().Stringize();
        }
        public List<Token> GetTokenCollection() => IRGenerator.GetTokenCollection();
        public List<Token> GetTokenCollection(out MugLexer lexer) => IRGenerator.GetTokenCollection(out lexer);
        public NamespaceNode GetNodeCollection() => IRGenerator.GetNodeCollection();
        public NamespaceNode GetNodeCollection(out MugParser parser) => IRGenerator.GetNodeCollection(out parser);
    }
}
