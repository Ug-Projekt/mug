using System;
class Mug
{
    static void Main(string[] args)
    {
        const string path = @"..\..\..\test\base.mug";
        compile(System.IO.Path.GetFullPath(path));
    }
    static void compile(string path)
    {
        Console.Title = "MugC";

        //Console.Write("TestingMugC@ ");
        var syntaxTree = Lexer.GetSyntaxTree(System.IO.File.ReadAllBytes(path));
        CompilationErrors.Except(true);

        //Console.WriteLine("SyntaxTree:");
        //syntaxTree.PrintTree();
        //Console.ReadKey();
        //Console.Clear();

        new GlobalParser().GetAbstractSyntaxTree(syntaxTree);

        //Console.WriteLine("AbstractSyntaxTree:");
        //abstractSyntaxTree.PrintTree();
        if (!GlobalParser.Functions.TryGetValue("main", out FunctionData entrypoint))
            CompilationErrors.Add(
                "Main Function Missing",
                "Main function is required as program entry point",
                "Add a main function to the program: `func main(args: str[]): ?` or `func main(): ?`", 0, null
                );
        CompilationErrors.Except(true);

        var module = new Module(System.IO.Path.GetFileNameWithoutExtension(path)).GetAssembly();
        CompilationErrors.Except(true);

        //Console.WriteLine("Assembly:");
        //Console.WriteLine(module);
        string il = System.IO.Path.ChangeExtension(path, "il");
        System.IO.File.WriteAllText(il, module);
        while (!System.IO.File.Exists(il))
        {
        }
        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo() { ArgumentList = { il }, FileName = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ilasm", UseShellExecute = true, WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden }
            );
        success(il);
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