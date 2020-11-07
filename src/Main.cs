using System;
class Mug
{
    static void Main(string[] args)
    {
        compile(System.IO.Path.GetFullPath(@"C:\Users\Mondelli\Desktop\MugProgrammingLanguage\MugProgrammingLanguage\test\base.mug"));
    }
    static void compile(string path)
    {
        Console.Title = "MugC";

        //Console.Write("TestingMugC@ ");
        var syntaxTree = Lexer.GetSyntaxTree(System.IO.File.ReadAllBytes(path));
    	CompilationErrors.Except(true);

        if (ask("Show Tokens"))
        {
        	Console.WriteLine("SyntaxTree:");
        	syntaxTree.PrintTree();
        	pause();
        }
        new GlobalParser().GetAbstractSyntaxTree(syntaxTree);
    	if (!GlobalParser.Functions.TryGetValue("main", out FunctionData entrypoint))
    	CompilationErrors.Add(
    		"Main Function Missing",
    		"Main function is required as program entry point",
    		"Add a main function to the program: `func main(args: str[]): ?` or `func main(): ?`", 0, null
	        );
    	CompilationErrors.Except(true);
		if (ask("Show Main Ast"))
        {
        	Console.WriteLine("AbstractSyntaxTree:");
        	GlobalParser.Functions["main"].Body.PrintTree();
		}
		if (ask("Generate Assembly"))
    	{
    	    var module = new Module(System.IO.Path.GetFileNameWithoutExtension(path)).GetAssembly();
        	CompilationErrors.Except(true);
        	if (ask("Show Assembly"))
    		{
	        	Console.WriteLine("Assembly:");
    	    	Console.WriteLine(module);
    		}
    		if (ask("Generate Executable"))
        		generate(System.IO.Path.ChangeExtension(path, "il"), System.IO.Path.ChangeExtension(path, "exe"), module);
	    }
    }
    static void pause()
    {
		Console.ReadKey();
    	Console.Clear();
    }
    static bool ask(string toAsk)
    {
    	Console.Write(toAsk+": ");
    	return Console.ReadKey().KeyChar == 's';
    }
    static void generate(string il, string executable, string module)
    {
    	System.IO.File.WriteAllText(il, module);
        while (!System.IO.File.Exists(il))
        {
        }
        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo() { ArgumentList = { il }, FileName = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ilasm", UseShellExecute = true, WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden }
            );
        while (!System.IO.File.Exists(executable))
        {

        }
        System.IO.File.Delete(il);
        success(executable);
    }
    static void success(string path)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write("Compilation Success: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(path);
        Console.ResetColor();
    }
}