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
        static String tempc => temp + ModuleName + ".c";
        static String tempexe => temp + ModuleName + ".exe";
        public CompilationUnit(string moduleName, string source)
        {
            ModuleName = moduleName;
            IRGenerator = new (moduleName, source);
        }
        public CompilationUnit(string path)
        {
            ModuleName = Path.GetFileNameWithoutExtension(path);
            IRGenerator = new (Path.GetFileNameWithoutExtension(path), File.ReadAllText(path));
        }
        public string CompileModule()
        {
            IRGenerator.Parser.Lexer.Tokenize();
            IRGenerator.Parser.Parse();
            return IRGenerator.Generate();
        }
        public void Compile(string filePathDestination = null)
        {
            if (filePathDestination is null)
                filePathDestination = ModuleName;
            if (File.Exists(tempc))
                File.Delete(tempc);
            if (File.Exists(tempexe))
                File.Delete(tempexe);
            if (File.Exists(filePathDestination))
                File.Delete(filePathDestination);
            var gen = CompileModule();
            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);
            File.WriteAllText(tempc, gen);
            while (!File.Exists(tempc));
            var clangCall = Process.Start("gcc", tempc + " -o " + tempexe);
            clangCall.WaitForExit();
            if (clangCall.ExitCode != 0)
                CompilationErrors.Throw("Extern Compiler: impossible to build due to prevoius errors");
            while (!File.Exists(tempexe));
            File.Delete(tempc);
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
