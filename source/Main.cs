using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Diagnostics;
using System.IO;



if (debug.isDebug())
{
    var testPath = $"C:/Users/{Environment.UserName}/Desktop/Mug/tests/LastUpdates.mug";
    var test = @"
var i: i32 = (new [chr, 1] { 2 })[0];
";

    try
    {
        //var lexer = new MugLexer(testPath, File.ReadAllText(testPath));
        var lexer = new MugLexer("test.mug", test);
        var tokens = lexer.Tokenize();
        var parser = new MugParser(lexer);
        var tree = parser.Parse();
        
        debug.print(tree.Stringize());
        //File.WriteAllText(Path.ChangeExtension(testPath, "mast"), tree.Stringize());
    }
    catch (CompilationException)
    {
    }
}