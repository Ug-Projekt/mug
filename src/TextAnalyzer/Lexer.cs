using System;
using System.Linq;
partial class Lexer {
    static bool PassingOnString { get; set; }
    static short CharIndex { get; set; }
    static short LineIndex { get; set; }
    static string Identifier { get; set; }
    static SyntaxTreeBuilder _syntaxTreeBuilder { get; set; }
    public static SyntaxTree GetSyntaxTree(byte[] source) {
        initializeComponents(source);

        while (LineIndex < SourceInfo.Source.Length) {
            while (CharIndex < SourceInfo.Source[LineIndex].Length) {
                if (PassingOnString) {
                    Identifier += SourceInfo.Source[LineIndex][CharIndex];
                    if (SourceInfo.Source[LineIndex][CharIndex] == '\'' || SourceInfo.Source[LineIndex][CharIndex] == '\"') {
                        if (!string.IsNullOrEmpty(Identifier) && !SyntaxRules.BuiltInKeyword.Contains(Identifier))
                            InsertIdentifierToST();
                        else
                            CheckIfIsKeyword();
                        PassingOnString = false;
                    }
                } else
                    ProcessCharType(SourceInfo.Source[LineIndex][CharIndex]);
                Advance();
            }
            AdvanceLine();
        }
        InsertEndOfFileToken();
        return _syntaxTreeBuilder.Build();
    }
    static void AdvanceLine() { LineIndex++; CharIndex = 0; }
    static void Advance() => CharIndex++;
    static void initializeComponents(byte[] source) {
        CompilationErrors.Reset();
        _syntaxTreeBuilder = new SyntaxTreeBuilder();
        SourceInfo.Source = SourceInfo.GetLinesFromSource(source);
        CharIndex = 0;
        LineIndex = 0;
        Identifier = "";
        PassingOnString = false;
    }
}