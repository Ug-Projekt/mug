using LLVMSharp;
using Mug.Compilation;
using System;
using System.IO;

try
{

#if DEBUG

    var test = @"
func fib(n: i32): i32 {
  if n < 2
    { return n; }
  return fib(n - 1) + fib(n - 2);
}

func main() {
}
";

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
    Console.WriteLine($"{(e.Lexer is not null ? $"(`{e.Lexer.Source[e.Bad]}`): " : "")}{e.Message}");
    /*if (e.Lexer is not null)
    {
        CompilationErrors.WriteModule(e.Lexer.ModuleName);
        CompilationErrors.WriteSourceLine(e.Bad, e.LineAt, e.Lexer.Source, e.Message);
    }
    else
        CompilationErrors.WriteFail(e.Message);*/
}