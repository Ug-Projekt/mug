using System;
class Mug
{
    static void Main(string[] args)
    {
        compile();
    }
    static void compile()
    {
        const string path = @"..\..\..\test\base.mug";
        Console.Title = "MugC";

        //Console.Write("TestingMugC@ ");
        var syntaxTree = Lexer.GetSyntaxTree(System.IO.File.ReadAllBytes(path));
        CompilationErrors.Except(true);

        //Console.WriteLine("SyntaxTree:");
        //syntaxTree.PrintTree();
        //Console.ReadKey();
        //Console.Clear();

        var abstractSyntaxTree = new GlobalParser().GetAbstractSyntaxTree(syntaxTree);
        CompilationErrors.Except(true);

        //Console.WriteLine("AbstractSyntaxTree:");
        //abstractSyntaxTree.PrintTree();

        ///*var Code = */new CodeGenerator().GetAssembly(abstractSyntaxTree);
        //CompilationErrors.Except(true);
    }
}