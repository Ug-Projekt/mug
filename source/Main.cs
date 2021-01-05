using Mug;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;


if (debug.isDebug())
{
    var test = @"
func main() {
    var index: u8;
    var size: u32;
}";

    var compUnit = new CompilationUnit("test.mug", test);
    var tokens = compUnit.GetTokenCollection(out MugLexer lexer);
    var tree = new MugParser(lexer);
    debug.print(tree.GetNodeCollection().Stringize());
    debug.print(string.Join("\n", tokens));
}