using Mug.Compilation;
using Mug.Models.Parser;
using System;

try
{

#if DEBUG

    var test = @"
# mug build file(test.mug)

type Person {
   name: str,
   age: Age
}

type P {
   a: Age
}

type Age { age: u8 }

func main() {
}
";
    var unit = new CompilationUnit("test", test);

    /*unit.IRGenerator.Parser.Lexer.Tokenize();
    Console.WriteLine(((INode)unit.IRGenerator.Parser.Parse()).Dump());*/

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
    // Console.WriteLine($"{(e.Lexer is not null ? $"(`{e.Lexer.Source[e.Bad]}`: {e.Bad} in {e.Lexer.ModuleName}): " : "")}{e.Message}");
    if (e.Lexer is not null)
    {
        CompilationErrors.WriteModule(e.Lexer.ModuleName, e.LineAt);
        CompilationErrors.WriteSourceLine(e.Bad, e.LineAt, e.Lexer.Source, e.Message);
    }
    else
        CompilationErrors.WriteFail(e.Message);
}