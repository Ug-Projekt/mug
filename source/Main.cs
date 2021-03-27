using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;

try
{

#if DEBUG

    var test = @"

func main() {
}

";

    var unit = new CompilationUnit("test", test, true);

    Console.WriteLine(
    unit.GenerateAST().Dump());
    // unit.Generate(true, true);

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
    // Console.WriteLine($"{(e.Lexer is not null ? $"(`{e.Lexer.Source[e.Bad]}`: {e.Bad} in {e.Lexer.ModuleName}): " : "")}{e.Message}");
    if (e.Lexer is not null)
    {
        CompilationErrors.WriteModule(e.Lexer.ModuleName, e.LineAt);
        CompilationErrors.WriteSourceLine(e.Bad, e.LineAt, e.Lexer.Source, e.Message);
    }
    else
        CompilationErrors.WriteFail(e.Message);
}