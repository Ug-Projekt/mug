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
  var x: i32;

  if x < 90 {
    x = 0;
    while x < 10 {
       if x != 1 { x++; }
    }
    x = 1;
  }
}
";
    var unit = new CompilationUnit("test", test);
    unit.Generate(true, true);

#else

    if (args.Length == 0)
        CompilationErrors.Throw("No arguments passed");

    for (int i = 0; i < args.Length; i++)
    {
        var unit = new CompilationUnit(args[i]);
        unit.Compile(3);
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