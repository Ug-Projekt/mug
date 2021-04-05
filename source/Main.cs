﻿using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using System;

try
{

#if DEBUG

    var test = @"

// func add<T>(a: T, b: T) { return a + b }
// func add(a: i32, b: i32) { return a + b }

func (a: u1) add(b: u8): u8 { return b }

func main() {
  var i: u8 = 10
  true.add(i)
}
";

    var unit = new CompilationUnit("test.mug", test, true);

    // unit.IRGenerator.Parser.Lexer.Tokenize().ForEach(token => Console.WriteLine(token));
    // Console.WriteLine(unit.GenerateAST().Dump());
    unit.Generate(true, true);

#else

    if (args.Length == 0)
        CompilationErrors.Throw("No arguments passed");

    var options = new CompilationFlags();

    options.SetArguments(args[1..]);

    options.InterpretAction(args[0]);

#endif
}
catch (CompilationException e)
{
    if (!e.IsGlobalError)
    {
        try
        {
            var errors = e.Lexer.DiagnosticBag.GetErrors();
            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                CompilationErrors.WriteSourceLineStyle(e.Lexer.ModuleName, error.Bad, error.LineAt(e.Lexer.Source), e.Lexer.Source, error.Message);
            }
        }
        catch
        {
            CompilationErrors.WriteFail(e.Lexer is not null ? e.Lexer.ModuleName : "", e.Message);
        }
    }
    else
        CompilationErrors.WriteFail(e.Lexer is not null ? e.Lexer.ModuleName : "", e.Message);
}