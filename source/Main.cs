using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.IO;

if (debug.isDebug())
{
    var testPath = $"C:/Users/{Environment.UserName}/Desktop/Mug/tests/ForStatement.mug";
    var test = @"
var x: i32 = 90;
";
    var lexer = new MugLexer("MagicNumber.mug", File.ReadAllText(testPath));
    lexer.Tokenize();
    var parser = new MugParser(lexer);
    var tree = parser.Parse();
    
    File.WriteAllText(Path.ChangeExtension(testPath, "mast"), tree.Stringize());
}