using Mug.Models.Lexer;
using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mug.Compilation
{
    public static class CompilationErrors
    {
        public static void Throw(params string[] error)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("Error");
            Console.ResetColor();
            Console.WriteLine(": "+string.Join("", error));
            throw new CompilationException(null, new(), string.Join("", error));
        }
        public static void Throw(this MugLexer Lexer, int pos, params string[] error)
        {
            int start = pos;
            int end = pos+1;
            while (start >= 0 && Lexer.Source[start] != '\n')
                start--;
            while (end < Lexer.Source.Length && Lexer.Source[end] != '\n')
                end++;
            WriteModule(Lexer.ModuleName);
            WriteSourceLine(pos - start - 1, (pos+1) - start - 1, CountLines(Lexer.Source, pos) + 1, Lexer.Source[(start + 1)..end], string.Join("", error));
            throw new CompilationException(null, new(pos, pos), string.Join("", error));
        }
        public static void Throw(this MugLexer Lexer, Token token, params string[] error)
        {
            int start = token.Position.Start.Value;
            int end = token.Position.End.Value;
            while (start >= 0 && Lexer.Source[start] != '\n')
                start--;
            while (end < Lexer.Source.Length && Lexer.Source[end] != '\n')
                end++;
            WriteModule(Lexer.ModuleName);
            WriteSourceLine(token.Position.Start.Value - start - 1, token.Position.End.Value - start - 1, CountLines(Lexer.Source, start+1), Lexer.Source[(start+1)..end], string.Join("", error));
            throw new CompilationException(null, token.Position, string.Join("", error));
        }
        public static void Throw(this MugParser Parser, INode node, params string[] error)
        {
            int start = node.Position.Start.Value;
            int end = node.Position.End.Value;
            while (start >= 0 && Parser.Lexer.Source[start] != '\n')
                start--;
            while (end < Parser.Lexer.Source.Length && Parser.Lexer.Source[end] != '\n')
                end++;
            WriteModule(Parser.Lexer.ModuleName);
            WriteSourceLine(node.Position.Start.Value - start - 1, node.Position.End.Value - start - 1, CountLines(Parser.Lexer.Source, start), Parser.Lexer.Source[(start + 1)..end], string.Join("", error));
            throw new CompilationException(null, node.Position, string.Join("", error));
        }
        public static void Throw(this MugLexer Lexer, Range position, params string[] error)
        {
            int start = position.Start.Value;
            int end = position.End.Value;
            while (start >= 0 && Lexer.Source[start] != '\n')
                start--;
            while (end < Lexer.Source.Length && Lexer.Source[end] != '\n')
                end++;
            WriteModule(Lexer.ModuleName);
            WriteSourceLine(position.Start.Value - start - 1, position.End.Value - start - 1, CountLines(Lexer.Source, start), Lexer.Source[(start + 1)..end], string.Join("", error));
            throw new CompilationException(null, position, string.Join("", error));
        }
        static void WriteModule(string moduleName)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(moduleName);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("]");
            Console.ResetColor();
        }
        static int CountLines(string source, int posStart)
        {
            int count = 1;
            for (; posStart >= 0; posStart--)
                if (source[posStart] == '\n')
                    count++;
            return count;
        }
        static void WriteSourceLine(int start, int end, int lineAt, string line, string error)
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
                + "^" + new string('~', line[start..end].Length-1));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(error);
            Console.ResetColor();
        }
    }
}
