using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;

try
{

#if DEBUG

    var test = @"

pub type Vec<T> { raw: [T], len: i32, cap: i32 }

pub func Vec<T>(): Vec<T> { return Vec<T>(1000); }

pub func Vec<T>(capacity: i32): Vec<T> {
  return new Vec<T> {
    raw: new [T] { },
    len: 0,
    cap: capacity
  };
}

pub func (self: *Vec<T>) push<T>(element: T) {
  (*self).raw[(*self).len] = element;
  (*self).len++;
}

pub func (self: *Vec<T>) pop<T>(): T {
  self.len--;
  return (*self).raw[(*self).len];
}

func main(): i32 {
var vec = Vec<i32>();
(&vec).push<i32>(1);
(&vec).pop<i32>();
return vec.len;
}

";

    var unit = new CompilationUnit("test", test);

    // Console.WriteLine(unit.GenerateAST().Dump());
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