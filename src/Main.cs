using System;
using System.Text;
class Mug {
    static void Main(string[] args) {
        compile();
    }
    static void compile() {
        const string path = @"..\..\..\Source\test\base.mug";
        Console.Title = "MugC";

        //Console.Write("TestingMugC@ ");
        var syntaxTree = Lexer.GetSyntaxTree(System.IO.File.ReadAllBytes(path));
        CompilationErrors.Except(true);

        Console.WriteLine("SyntaxTree:");
        syntaxTree.PrintTree();

        var abstractSyntaxTree = new GlobalParser().GetAbstractSyntaxTree(syntaxTree);
        CompilationErrors.Except(true);

        Console.WriteLine("AbstractSyntaxTree:");
        abstractSyntaxTree.PrintTree();
    }
}