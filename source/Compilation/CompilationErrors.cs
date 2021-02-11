using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;

namespace Mug.Compilation
{
    public static class CompilationErrors
    {
        public static bool PrintErrors { get; set; } = true;

        public static void Throw(params string[] error)
        {
            if (PrintErrors)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("Error");
                Console.ResetColor();
                Console.WriteLine(": " + string.Join("", error));
            }
            throw new CompilationException(null, new(), string.Join("", error));
        }
        public static void Throw(this MugLexer Lexer, int pos, params string[] error)
        {
            Lexer.Throw(pos..(pos+1), error);
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
            if (PrintErrors)
            {
                WriteModule(Lexer.ModuleName);
                WriteSourceLine(position.Start.Value - start - 1, position.End.Value - start - 1, CountLines(Lexer.Source, start), Lexer.Source[(start + 1)..end], err);
            }
            throw new CompilationException(Lexer, position, err);
        }

        private static void WriteModule(string moduleName)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(moduleName);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("]");
            Console.ResetColor();
        }

        private static int CountLines(string source, int posStart)
        {
            int count = 1;
            for (; posStart >= 0; posStart--)
                if (source[posStart] == '\n')
                    count++;
            return count;
        }

        private static void WriteSourceLine(int start, int end, int lineAt, string line, string error)
        {
            Console.WriteLine($"@Raw(Line: {lineAt}, Position: ({start}..{end}))");
            Console.Write(lineAt);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(" | ");
            Console.ResetColor();
            Console.Write(line[..start].Replace("\t", " "));
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(line[start..end].Replace("\t", " "));
            Console.ResetColor();
            Console.Write("{0}\n{1} ", line[end..].Replace("\t", " "), new string(' ', lineAt.ToString().Length + 3 + line[..start].Length)
                + "^" + new string('~', line[start..end].Length - 1));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(error);
            Console.ResetColor();
        }
    }
}
