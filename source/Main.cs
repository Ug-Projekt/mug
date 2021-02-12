using LLVMSharp;
using Mug.Compilation;

try
{
    if (debug.isDebug())
    {
        var test = @"
func add(a: i32, b: i32): i32 {
  return a+b;
}
func main(): i32 {
  return add(2, 2);
}
";

        var unit = new CompilationUnit("test", test);
        unit.Generate();

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
    if (e.Lexer is not null)
    {
        CompilationErrors.WriteModule(e.Lexer.ModuleName);
        CompilationErrors.WriteSourceLine(e.Bad, e.LineAt, e.Lexer.Source, e.Message);
    }
    else
        CompilationErrors.WriteFail(e.Message);
}