using Mug.Models.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug
{
    public static class CompilationErrors
    {
        public static MugLexer Lexer;
        public static void Throw(ref Token token, params string[] error)
        {
            int start = token.Position.Start.Value;
            int end = token.Position.End.Value;
            while (start >= 0 && Lexer.Source[start] != '\n')
                start--;
            while (end < Lexer.Source.Length && Lexer.Source[end] != '\n')
                end++;
            WriteSourceLine(ref token, Lexer.Source[(start+1)..end], string.Join("", error));
        }
        static void WriteSourceLine(ref Token token, string line, string error)
        {
            Console.Write("{0} | {1}", token.LineAt+1, line[..token.Position.Start]);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(line[token.Position.Start..token.Position.End]);
            Console.ResetColor();
            Console.WriteLine("{0}\n{1} {2}", line[token.Position.End..], new string(' ', (token.LineAt + 1).ToString().Length + 3 + line[..token.Position.Start].Length)
                + "^" + new string('~', line[token.Position.Start..token.Position.End].Length-1), error);
        }
    }
}
