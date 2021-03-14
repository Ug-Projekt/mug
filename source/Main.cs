using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;

try
{

#if DEBUG

    var test = @"

func puts(text: str);
func exit(code: i32);

func panic(error: str) {
  puts(error);
  exit(1);
}

enum UserKind: u8 {
  Normal: 0,
  Admin: 1,
  Bot: 2
}


type Result<T> {
  is_err: u1,
  result: T
}

func userkind_from_category(category: chr): Result<UserKind> {
  if   category == 'a' { return new Result<UserKind> { result: UserKind.Normal }; }
  elif category == 'b' { return new Result<UserKind> { result: UserKind.Admin  }; }
  elif category == 'c' { return new Result<UserKind> { result: UserKind.Bot    }; }
  return new Result<UserKind> { is_err: true };
}

func main(): i32 {
  var result = userkind_from_category('a');

  if result.is_err {
    panic(""Unable to recognize userkind"");
  }

  return (result.result as u8) as i32;
}

"; // as operator broken or (new Struct { }).field

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