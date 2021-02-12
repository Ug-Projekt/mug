using LLVMSharp;
using Mug.Compilation;
using System;

try
{
    if (debug.isDebug())
    {
        var test = @"
func putchar(char: chr);

func add(a: i32, b: i32): chr {
  return (a + b) as chr;
}
func main()
{
  putchar(add(2, 2));
  return;
}";

        var unit = new CompilationUnit("test", test);
        unit.Generate(true);

        LLVM.DumpModule(unit.IRGenerator.Module);
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