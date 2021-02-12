using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;

namespace Mug.Compilation
{
    public static class CompilationErrors
    {
        public static void Throw(params string[] error)
        {
            throw new CompilationException(null, new(), string.Join("", error));
        }
        public static void Throw(this MugLexer Lexer, int pos, params string[] error)
        {
            Lexer.Throw(pos..(pos + 1), error);
        }
        public static void Throw(this MugLexer Lexer, Token token, params string[] error)
        {
            Lexer.Throw(token.Position, error);
        }
        public static void Throw(this MugParser Parser, INode node, params string[] error)
        {
            Parser.Lexer.Throw(node.Position, error);
        }
        public static void Throw(this MugLexer Lexer, Range position, params string[] error)
        {
            int start = position.Start.Value;
            int end = position.End.Value;
            while (start >= 0 && Lexer.Source[start] != '\n')
                start--;
            while (end < Lexer.Source.Length && Lexer.Source[end] != '\n')
                end++;
            var err = string.Join("", error);

            throw new CompilationException(Lexer, position, err);
        }

        public static void WriteModule(string moduleName)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(moduleName);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("]");
            Console.ResetColor();
        }

        public static int CountLines(string source, int posStart)
        {
            int count = 1;
            for (; posStart >= 0; posStart--)
                if (source[posStart] == '\n')
                    count++;
            return count;
        }

        public static void WriteSourceLine(Range position, int lineAt, string source, string error)
        {
            Console.WriteLine($"@Raw(Line: {lineAt}, Position: ({position.Start}..{position.End}))");
            Console.Write(lineAt);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(" | ");
            Console.ResetColor();
            Console.Write(source[..position.Start].Replace("\t", " "));
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(source[position.Start..position.End].Replace("\t", " "));
            Console.ResetColor();
            Console.Write("{0}\n{1} ", source[position.End..].Replace("\t", " "), new string(' ', lineAt.ToString().Length + 3 + source[..position.Start].Length)
                + "^" + new string('~', source[position.Start..position.End].Length - 1));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        public static void WriteFail(string error)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("Error");
            Console.ResetColor();
            Console.WriteLine(": " + string.Join("", error));
        }
    }
}
