using LLVMSharp;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Mug.Compilation
{
    public class CompilationUnit
    {
        public IRGenerator IRGenerator;
        public CompilationUnit(string moduleName, string source)
        {
            IRGenerator = new(moduleName, source);
        }
        public CompilationUnit(string path)
        {
            IRGenerator = new(Path.GetFileNameWithoutExtension(path), File.ReadAllText(path));
        }
        public void Compile(int optimizazioneLevel)
        {
            Generate();

            if (LLVM.VerifyModule(IRGenerator.Module, LLVMVerifierFailureAction.LLVMReturnStatusAction, out string err))
                CompilationErrors.Throw(err);

            CompileModule(optimizazioneLevel);
        }

        private void CompileModule(int optimizazioneLevel)
        {
            writeFile(IRGenerator.Module, optimizazioneLevel,
                Path.ChangeExtension(IRGenerator.Parser.Lexer.ModuleName, "bc"));

            static void exit(string error)
            {
                CompilationErrors.Throw(error);
            }

            static void nothing()
            {

            }

            static void writeFile(LLVMModuleRef module, int optimizazioneLevel, string output, string clangFilename = "clang")
            {
                if (File.Exists(output)) File.Delete(output);

                if (LLVM.WriteBitcodeToFile(module, output) != 0)
                    exit("Error writing to file");

                while (!File.Exists(output)) nothing();

                callClang(output, clangFilename, optimizazioneLevel);

                if (File.Exists(output)) File.Delete(output);
            }

            static string getExecutableExtension()
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32NT ? "exe" : null);
            }

            static void callClang(string outputFilename, string clangFilename, int optimizazioneLevel)
            {
                var clang = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = clangFilename,
                        Arguments = $"{outputFilename} -O{optimizazioneLevel} -o {Path.ChangeExtension(outputFilename, getExecutableExtension())}",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        ErrorDialog = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    });
                clang.WaitForExit();
            }
        }

        public void Generate()
        {
            IRGenerator.Parser.Lexer.Tokenize();
            IRGenerator.Parser.Parse();
            IRGenerator.Generate();

            if (LLVM.VerifyModule(IRGenerator.Module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out string error))
                CompilationErrors.Throw($"External compiler error: {error}");
        }

        public string GetStringAST()
        {
            return GetNodeCollection().Dump();
        }
        public List<Token> GetTokenCollection() => IRGenerator.GetTokenCollection();
        public List<Token> GetTokenCollection(out MugLexer lexer) => IRGenerator.GetTokenCollection(out lexer);
        public NamespaceNode GetNodeCollection() => IRGenerator.GetNodeCollection();
        public NamespaceNode GetNodeCollection(out MugParser parser) => IRGenerator.GetNodeCollection(out parser);
    }
}
