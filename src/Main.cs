using System;
using System.Text;
class Mug {
    static void Main(string[] args) {
        Console.Title = "MugC";
        bool isBody = false;
        string program = "";
        while (true) {
            if (isBody) {
                Console.Write("... ");
                program += Console.ReadLine();
                if (program[^1] == '}')
                    isBody = false;
            } else {
                Console.Write("> ");
                program += Console.ReadLine();
                if (program[^1] == '{')
                    isBody = true;
                else {
                    var result = new Parser().GetAbstractSyntaxTree(Lexer.GetSyntaxTree(Encoding.ASCII.GetBytes(program)));
                    program = "";
                    CompilationErrors.Except(false);
                }
            }
        }
    }
}