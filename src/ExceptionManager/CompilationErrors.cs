using System;
using System.Runtime;

class CompilationErrors {
    static Errors Exceptions = new Errors();
    public static void Add(string Error, string Reason, string TryTo, short lineIndex, short charIndex) => Exceptions.Add(Error, Reason, TryTo, lineIndex, charIndex);
    public static void Except() {
        if (Exceptions.Count > 0) {
            for (short i = 0; i < Exceptions.Count; i++) {
                printLine(SourceInfo.GetLine(Exceptions[i].Item4, Exceptions[i].Item5), Exceptions[i].Item4, Exceptions[i].Item5);
                printError(Exceptions[i].Item1);
                printReason(Exceptions[i].Item2);
                printTryTo(Exceptions[i].Item3);
                Console.ResetColor();
            }
            Environment.Exit(1);
        }
    }
    static void printError(string err) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("#[Error]: ");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(err);
    }
    static void printReason(string res) {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("\tErrorReason: ");
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(res);
    }
    static void printTryTo(string tip) {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\tCompilationTip: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(tip);
    }
    static void printLine(string[] line, short lineIndex, short charIndex) {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Line:("+lineIndex+", "+charIndex+") " + line[0]);
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write(line[1]);
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(line[2]);
    }
}