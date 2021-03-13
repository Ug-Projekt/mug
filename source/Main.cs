using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;

try
{

#if DEBUG

    var test = @"

type J<H> {
  a: H
}

type C<G> {
  a: J<G>
}

type Vec<T, T1> {
  b: T,
  c: C<T1>
}

func main() {
  var x = new Vec<u8, i32> { b: 1 as u8 };
}

";

    var unit = new CompilationUnit("test", test);

    /*unit.IRGenerator.Parser.Lexer.Tokenize();
    Console.WriteLine(((INode)unit.IRGenerator.Parser.Parse()).Dump());*/
    //Console.WriteLine(unit.GenerateAST().Dump());
    unit.Generate(true, true);

#else

    if (args.Length == 0)
        CompilationErrors.Throw("No arguments passed");

    var options = new CompilationFlags();

    options.SetArguments(args[1..]);

    options.SetCompilationAction(args[0]);

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