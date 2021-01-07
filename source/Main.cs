using Mug;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;

if (debug.isDebug())
{
    // var testPath = $"C:/Users/{Environment.UserName}/Desktop/Mug/tests/add.mug";
    var test = @"
func main(args: [str]) {
    if (true) {
    }
    elif (!((2+2)*(92-2) == len(args))) {
    }
    else {
    }
}
";
    var compUnit = new CompilationUnit("test.mug", test);
    var tokens = compUnit.GetTokenCollection(out MugLexer lexer);
    var tree = new MugParser(lexer).GetNodeCollection();

    debug.print("SyntaxTree:\n", tree.Stringize());
    //debug.print("TokenTree:\n", string.Join("\n", tokens));
}