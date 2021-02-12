using LLVMSharp;
using Mug.Compilation;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.IO;

try
{
    if (debug.isDebug())
    {
        var test = @"
func main(): i32 {
  var x = 0;
  return x;
}";

        var lexer = new MugLexer("test", test);
        lexer.Tokenize();
        var parser = new MugParser(lexer);
        parser.Parse();
        var generator = new IRGenerator(parser);
        generator.Generate();

        LLVM.DumpModule(generator.Module);
        LLVM.VerifyModule(generator.Module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out string err);
    }
    else
    {
        if (args.Length == 0)
            CompilationErrors.Throw("No arguments passed");
        for (int i = 0; i < args.Length; i++)
        {
            var unit = new CompilationUnit(args[i]);
            unit.Compile(0);
        }
    }
}
catch (CompilationException)
{
    Console.WriteLine("Cannot build due to previous errors");
    Environment.Exit(1);
}
