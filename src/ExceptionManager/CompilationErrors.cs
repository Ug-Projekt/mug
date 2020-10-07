using System;

class CompilationErrors {
    static Errors Exceptions = new Errors();
    public static void Add(string Error, string Reason, string TryTo, short lineIndex, short? charIndex) {
        if (!Exceptions.Contains(Error, Reason, TryTo, lineIndex, charIndex))
            Exceptions.Add(Error, Reason, TryTo, lineIndex, charIndex);
    }
    public static void Reset() => Exceptions.Clear();
    public static void Except(bool killPocess) {
        if (Exceptions.Count > 0) {
            for (short i = 0; i < Exceptions.Count; i++) {
                if (Exceptions[i].Item5 == null)
                    printLine(SourceInfo.GetFlatLine(Exceptions[i].Item4), Exceptions[i].Item5);
                else
                    printLine(SourceInfo.GetLine(Exceptions[i].Item4, Exceptions[i].Item5), Exceptions[i].Item4, Exceptions[i].Item5);
                printError(Exceptions[i].Item1);
                printReason(Exceptions[i].Item2);
                printTryTo(Exceptions[i].Item3);
                Console.ResetColor();
            }
            if (killPocess)
                Environment.Exit(1);
        }
    }
    static void printError(string err) {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write("#[Error] -> ");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(err);
    }
    static void printReason(string res) {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write("\tErrorReason: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(res);
    }
    static void printTryTo(string tip) {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write("\tCompilationTip: ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(tip);
    }
    static void printLine(string[] line, short lineIndex, short? charIndex) {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write("Line:(" + (lineIndex + 1) + ", " + (charIndex + 1) + ") ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(line[0]);
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.Write(line[1]);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(line[2]);
    }
    static void printLine(string line, short? lineIndex) {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write("Line:(" + (Convert.ToInt16(lineIndex)+1) + ") ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(line);
    }
}