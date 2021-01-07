using Mug;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;

string buildTest()
{
    string code = "";
    string lastInput = "";
    do
    {
        Console.Write(".. ");
        lastInput = Console.ReadLine();
        code += lastInput+"\n";
    } while (lastInput != "");
    return code;
}

if (debug.isDebug())
{
    // var testPath = $"C:/Users/{Environment.UserName}/Desktop/Mug/tests/add.mug";
    var test = @"
func main(a: str = ) {
}
";
    //test = buildTest();
    var compUnit = new CompilationUnit("test.mug", test);
    var tokens = compUnit.GetTokenCollection(out MugLexer lexer);
    var tree = new MugParser(lexer).GetNodeCollection();

    debug.print(tree.Stringize());
    //debug.print("TokenTree:\n", string.Join("\n", tokens));
}