﻿using LLVMSharp;
using Mug.Compilation;
using Mug.Models.Parser;
using System;
using System.IO;

try
{

#if DEBUG

    var test = @"
func main() {
   
}
";
    /*var parser = new MugParser("test", test);
    Console.WriteLine(parser.Parse().Dump());*/

    var unit = new CompilationUnit("test", test);
    unit.Generate(true);

    unit.IRGenerator.Module.Dump();

#else

    if (args.Length == 0)
            CompilationErrors.Throw("No arguments passed");

        for (int i = 0; i < args.Length; i++)
        {
            var unit = new CompilationUnit(args[i]);
            unit.Compile(0);
        }

#endif
}
catch (CompilationException e)
{
    Console.WriteLine($"{(e.Lexer is not null ? $"(`{e.Lexer.Source[e.Bad]}`: {e.Bad} in {e.Lexer.ModuleName}): " : "")}{e.Message}");
    /*if (e.Lexer is not null)
    {
        CompilationErrors.WriteModule(e.Lexer.ModuleName);
        CompilationErrors.WriteSourceLine(e.Bad, e.LineAt, e.Lexer.Source, e.Message);
    }
    else
        CompilationErrors.WriteFail(e.Message);*/
}