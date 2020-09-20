using System;
using System.Text;
class Mug {
    static void Main(string[] args) {
        Console.Title = "MugC";
        while (true) {
            Console.Write("> ");
            var result = new Parser().GetAbstractSyntaxTree(Lexer.GetSyntaxTree(Encoding.ASCII.GetBytes(Console.ReadLine())));
            //result.PrintTree();
            
            CompilationErrors.Except(false);
        }
    }
}