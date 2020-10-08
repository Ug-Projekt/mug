using System;
using System.Text;
class Mug {
    static void Main(string[] args) {
        Console.Title = "MugC";
        while (true) {
            Console.Write("TestingMugC@ ");
            var result = new Parser().GetAbstractSyntaxTree(
                Lexer.GetSyntaxTree(
                    System.IO.File.ReadAllBytes("file.mug")
                    //Encoding.ASCII.GetBytes(Console.ReadLine().Replace("\\n", "\n"))
                )
            );
            //var result = new Parser().GetAbstractSyntaxTree(Lexer.GetSyntaxTree(Encoding.ASCII.GetBytes(Console.ReadLine().Replace("\\n", "\n"))));
            result.PrintTree();
        }
    }
}