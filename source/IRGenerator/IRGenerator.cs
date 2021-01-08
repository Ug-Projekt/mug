using LLVMSharp;
using Mug.Compilation;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public NamespaceNode Module;
        MugParser Parser;
        MugEmitter Emitter;
        LLVMBool Success = new LLVMBool(0);
        public IRGenerator(string moduleName, string source)
        {
            Parser = new (moduleName, source);
            Module = Parser.Parse();
            Emitter = new(moduleName);
            Module = new MugParser(moduleName, source).Parse();
        }
        void Process()
        {

        }
        public LLVMModuleRef Generate()
        {
            Emitter.DefineFunction();
            return Emitter.LLVMModule;
        }
        public void Compile()
        {
            var mod = Generate();
            if (LLVM.VerifyModule(mod, LLVMVerifierFailureAction.LLVMPrintMessageAction, out var error) != Success)
                CompilationErrors.Throw("LLVM Binder: ", error);

            LLVM.LinkInMCJIT();

            LLVM.InitializeX86TargetMC();
            LLVM.InitializeX86Target();
            LLVM.InitializeX86TargetInfo();
            LLVM.InitializeX86AsmParser();
            LLVM.InitializeX86AsmPrinter();

            LLVMMCJITCompilerOptions options = new LLVMMCJITCompilerOptions { NoFramePointerElim = 1 };
            LLVM.InitializeMCJITCompilerOptions(options);
            if (LLVM.CreateMCJITCompilerForModule(out var engine, mod, options, out error) != Success)
                CompilationErrors.Throw("LLVM Binder: ", error);

            if (LLVM.WriteBitcodeToFile(mod, CompilationUnit.tempbc) != 0)
                CompilationErrors.Throw("Code Writer: failed writing generated code");

            LLVM.DumpModule(mod);

            LLVM.DisposeExecutionEngine(engine);
        }
        public List<Token> GetTokenCollection() => Parser.GetTokenCollection();
        public List<Token> GetTokenCollection(out MugLexer lexer) => Parser.GetTokenCollection(out lexer);
        public NamespaceNode GetNodeCollection() => Parser.Parse();
        public NamespaceNode GetNodeCollection(out MugParser parser)
        {
            var nodes = Parser.Parse();
            parser = Parser;
            return nodes;
        }
    }
}
