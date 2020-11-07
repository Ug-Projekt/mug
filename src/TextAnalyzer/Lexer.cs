using System;
using System.Linq;
partial class Lexer
{
    static bool PassingOnString { get; set; }
    static short CharIndex { get; set; }
    static short LineIndex { get; set; }
    static string Identifier { get; set; }
    static SyntaxTreeBuilder _syntaxTreeBuilder { get; set; }
    public static SyntaxTree GetSyntaxTree(byte[] source)
    {
        initializeComponents(source);

        while (LineIndex < SourceInfo.Source.Length)
        {
            while (CharIndex < SourceInfo.Source[LineIndex].Length)
            {
                if (PassingOnString)
                {
                    Identifier += SourceInfo.Source[LineIndex][CharIndex];
                    if (SourceInfo.Source[LineIndex][CharIndex] == '\"')
                    {
                        if (!string.IsNullOrEmpty(Identifier) && !SyntaxRules.BuiltInKeyword.Contains(Identifier))
                            InsertIdentifierToST();
                        else
                            CheckIfIsKeyword();
                        PassingOnString = false;
                    }
                    else if (SourceInfo.Source[LineIndex][CharIndex] == '\\')
                    {
                        Advance();
                        Identifier = Identifier.Remove(Identifier.Length - 1) + SourceInfo.Source[LineIndex][CharIndex];
                    }
                }
                else
                    ProcessCharType(SourceInfo.Source[LineIndex][CharIndex]);
                Advance();
            }
            AdvanceLine();
        }
        if (Identifier != "")
            InsertIdentifierToST();
        InsertEndOfFileToken();
        return _syntaxTreeBuilder.NormalizeAndBuild();
    }
    static void AdvanceLine() { LineIndex++; CharIndex = 0; }
    static void Advance() => CharIndex++;
    static void Advance(short count) => CharIndex += count;
    static void initializeComponents(byte[] source)
    {
        CompilationErrors.Reset();
        _syntaxTreeBuilder = new SyntaxTreeBuilder();
        SourceInfo.Source = SourceInfo.GetLinesFromSource(source);
        CharIndex = 0;
        LineIndex = 0;
        Identifier = "";
        PassingOnString = false;
    }
}