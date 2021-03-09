using Mug.Compilation;
using Mug.Models.Parser;
using System;

try
{

#if DEBUG

    var test = @"
type Age { age: u8 }

type Person {
   name: str,
   age: Age
}

@[inline(true)]
func Person(name: str, age: u8): Person { return new Person { name: name, age: new Age { age: age } }; }

func main(): i32 {
  var me = Person(""carpal"", 16 as u8);
  var x = 1 as u8;
  me.age.age = x + (2 as u8);
  return me.age.age as i32;
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