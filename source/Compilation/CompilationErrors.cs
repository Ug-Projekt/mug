﻿using Mug.Models.Lexer;
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
            Console.Write("Internal Error: ");
            Console.ResetColor();
            Console.WriteLine(string.Join("", error));
            throw new CompilationException(null, new(), string.Join("", error));
        }
        public static void Throw(this MugLexer Lexer, int pos, int lineAt, params string[] error)
        {
            int start = pos;
            int end = pos+1;
            while (start >= 0 && Lexer.Source[start] != '\n')
                start--;
            while (end < Lexer.Source.Length && Lexer.Source[end] != '\n')
                end++;
            WriteModule(Lexer.ModuleName);
            WriteSourceLine(pos - start - 1, (pos+1) - start - 1, lineAt + 1, Lexer.Source[(start + 1)..end], string.Join("", error));
            throw new CompilationException(null, new(lineAt, TokenKind.Bad, null, new(pos, pos)), string.Join("", error));
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
            WriteSourceLine(token.Position.Start.Value - start - 1, token.Position.End.Value - start - 1, token.LineAt+1, Lexer.Source[(start+1)..end], string.Join("", error));
            throw new CompilationException(null, token, string.Join("", error));
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
