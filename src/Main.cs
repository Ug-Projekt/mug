using System;
using System.Text;

class Mug {
    static void Main(string[] args){
        while (true) {
            Console.Write("> ");
            var result = Lexer.GetSyntaxTree(Encoding.ASCII.GetBytes(Console.ReadLine()));
            for (int i = 0; i < result.Count; i++)
                Console.WriteLine("Line:({2}) Token: {0}, {1}", result[i].Item1, (result[i].Item2 != null) ? result[i].Item2:"", result[i].Item3);
            CompilationErrors.Except();
        }
    }
}