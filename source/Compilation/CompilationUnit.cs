﻿using LLVMSharp.Interop;
using Mug.Models.Generator;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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
            if (!File.Exists(path))
                CompilationErrors.Throw($"Unable to open path: `{path}`");

            IRGenerator = new(path, File.ReadAllText(path));
        }

        public void Compile(int optimizazioneLevel)
        {
            // generates the bytecode
            Generate();

            CompileModule(optimizazioneLevel);
        }

        /// <summary>
        /// writes the llvm module to a file and calls the llvm compiler on it
        /// </summary>
        private void CompileModule(int optimizazioneLevel)
        {
            // then this path will be taken from a configuration file
            const string clangStandardPath = "C:/Program Files/LLVM/bin/clang.exe";

            // checks the clang execuatble exists
            if (!File.Exists(clangStandardPath))
                CompilationErrors.Throw($"Cannot find the clang executable at: `{clangStandardPath}`");

            writeFile(IRGenerator.Module, optimizazioneLevel,
                Path.ChangeExtension(IRGenerator.Parser.Lexer.ModuleName, "bc"), clangStandardPath);

            static void nothing()
            {
                // called to avoid misunderstanding while reading the code
            }

            /// writes the bytecode to a file, calls clang on it and deletes the bytecode file
            static void writeFile(LLVMModuleRef module, int optimizazioneLevel, string output, string clangFilename)
            {
                // deletes a possible file named in the same way as the result, to avoid bugs in the while under
                if (File.Exists(output)) File.Delete(output);

                // writes the module to a file
                if (module.WriteBitcodeToFile(output) != 0)
                    CompilationErrors.Throw("Error writing to file");

                // the program goes in this keeps the program waiting until it finds the file containing the compiled program
                while (!File.Exists(output)) nothing();

                // clang compiler
                callClang(output, clangFilename, optimizazioneLevel);

                // deletes the bytecode file, now remains only the executable generated by clang
                if (File.Exists(output)) File.Delete(output);
            }

            /// unix executables have no extension
            static string getExecutableExtension()
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT ? "exe" : null;
            }

            static void callClang(string outputFilename, string clangFilename, int optimizazioneLevel)
            {
                // call clang
                var clang = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = clangFilename,
                        Arguments = $"{outputFilename} -O{optimizazioneLevel} -o {Path.ChangeExtension(outputFilename, getExecutableExtension())}",
                        // invisible window
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        ErrorDialog = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    });

                // the program will wait until clang works
                clang.WaitForExit();
                if (clang.ExitCode != 0)
                    CompilationErrors.Throw("External compiler: ", clang.StandardOutput.ReadToEnd());
            }
        }

        /// <param name="verifyLLVMModule">
        /// false only when debugs to see the module when llvm finds an error
        /// </param>
        public void Generate(bool verifyLLVMModule = true, bool dumpModule = false)
        {
            IRGenerator.Parser.Lexer.Tokenize();
            IRGenerator.Parser.Parse();
            IRGenerator.Generate();

            if (dumpModule)
                IRGenerator.Module.Dump();

            if (verifyLLVMModule)
                if (!IRGenerator.Module.TryVerify(LLVMVerifierFailureAction.LLVMReturnStatusAction, out var error))
                    CompilationErrors.Throw($"Cannot build due to external compiler error: {error}");
        }
    }
}
