using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;

try
{

#if DEBUG

    var test = @"
# import panic;
#[
pub enum VecErrors: str {
  OutOfBounds: ""Index was out of bounds of the vector""
}

pub type Vec<T> {
  arr: [T],
  cap: i32,
  len: i32
}

func Vec<T>(capacity: i32) { return new Vec<T> { cap: capacity, len: 0 }; }

func `[]`<T>(self: Vec<T>, index: i32): T {
  if index >= self.len { panic(VecErrors.OutOfBounds as str); }
  return self.arr[index];
}
]#
func main() {
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