using Mug.Compilation;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Diagnostics;
using System.IO;


try
{
    if (debug.isDebug())
    {
        var testPath = $"C:/Users/{Environment.UserName}/Desktop/Mug/tests/LastUpdates.mug";
        var test = @"
func main()
{
    print(""d"");
}
";

        
        //var lexer = new MugLexer(testPath, File.ReadAllText(testPath));
        var lexer = new MugLexer("test", test);
        lexer.Tokenize();
        var parser = new MugParser(lexer);
        parser.Parse();
        var generator = new IRGenerator(parser);
        var gen = generator.Generate();

        //debug.print(parser.Module.Stringize());
        //foreach (var member in generator.RedefinitionTable)
            //debug.print(member.Key, " -> ", member.Value);
        debug.print(gen);
        //File.WriteAllText(Path.ChangeExtension(testPath, "mast"), tree.Stringize());

    }
    else
    {
        if (args.Length == 0)
            CompilationErrors.Throw("No arguments passed");
        for (int i = 0; i < args.Length; i++)
        {
            var unit = new CompilationUnit(args[i]);
            unit.Compile(Path.ChangeExtension(args[i], "exe"));
        }
    }
}
catch (CompilationException)
{
    Console.WriteLine("Cannot build due to previous errors");
    Environment.Exit(1);
}