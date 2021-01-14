using Mug.Compilation;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Diagnostics;
using System.IO;



if (debug.isDebug())
{
    var testPath = $"C:/Users/{Environment.UserName}/Desktop/Mug/tests/LastUpdates.mug";
    var test = @"
func main(): i32
{
}
";

    try
    {
        //var unit = new CompilationUnit("test", test);
        //unit.Compile("C:/Users/Mondelli/Desktop/a.exe");
        //var lexer = new MugLexer(testPath, File.ReadAllText(testPath));
        var lexer = new MugLexer("test", test);
        lexer.Tokenize();
        var parser = new MugParser(lexer);
        parser.Parse();
        var generator = new IRGenerator(parser);
        var gen = generator.Generate();

        //debug.print(parser.Module.Stringize());
        //foreach (var member in generator.SymbolTable)
        //    debug.print(member.Key, " -> ", ((INode)member.Value).Stringize());
        debug.print(gen);
        //File.WriteAllText(Path.ChangeExtension(testPath, "mast"), tree.Stringize());
    }
    catch (CompilationException)
    {
    }
}