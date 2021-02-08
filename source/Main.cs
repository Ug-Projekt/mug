﻿using LLVMSharp;
using Mug.Compilation;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;


try
{
    if (debug.isDebug())
    {
        var testPath = $"C:/Users/{Environment.UserName}/Desktop/Mug/tests/.mug";
        var test = @"
func main(argcount: i32): i32 {
  return argcount-1;
}";

        //var lexer = new MugLexer(testPath, File.ReadAllText(testPath));
        var lexer = new MugLexer("test", test);
        lexer.Tokenize();
        var parser = new MugParser(lexer);
        parser.Parse();
        var generator = new IRGenerator(parser);
        generator.Generate();
        
        LLVM.DumpModule(generator.Module);
        LLVM.VerifyModule(generator.Module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out string err);
        //debug.print(parser.Module.Stringize());
        //foreach (var member in generator.RedefinitionTable)
        //debug.print(member.Key, " -> ", member.Value);
        //File.WriteAllText(Path.ChangeExtension(testPath, "mast"), tree.Stringize());
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