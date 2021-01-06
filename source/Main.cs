using Mug;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;

if (debug.isDebug())
{
    var test = @"
func main(args: [[str]]) {
    var index: u8 = 1*2*(10*(1+2+3+5-2));
}
func add(a: i32, b: i32): i32 {
    return a+b;
}
";
    var compUnit = new CompilationUnit("test.mug", test);
    var tokens = compUnit.GetTokenCollection(out MugLexer lexer);
    var tree = new MugParser(lexer).GetNodeCollection();

    debug.print("SyntaxTree:\n", tree.Stringize());
    debug.print("TokenTree:\n", string.Join("\n", tokens));
}