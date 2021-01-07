using Mug;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.IO;

if (debug.isDebug())
{
    var testPath = $"C:/Users/{Environment.UserName}/Desktop/Mug/tests/ifelse.mug";
    var test = @"
var x: i32 = 90;
";
    var compUnit = new CompilationUnit(testPath);
    var tokens = compUnit.GetTokenCollection(out MugLexer lexer);
    var tree = new MugParser(lexer).GetNodeCollection();

    File.WriteAllText(Path.ChangeExtension(testPath, "mast"), tree.Stringize());
    //debug.print("TokenTree:\n", string.Join("\n", tokens));rig
}