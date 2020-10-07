using System;
using System.Collections.Generic;
struct SourceInfo {
    public static string[] Source { get; set; }
    public static string[] GetLine(int lineIndex, short? charIndex) => new string[3] { Source[lineIndex].Substring(0, Convert.ToInt16(charIndex)), Source[lineIndex][Convert.ToInt16(charIndex)].ToString(), Source[lineIndex].Substring(Convert.ToInt16(charIndex + 1)) };
    public static string GetFlatLine(int lineIndex) { if (lineIndex >= Source.Length) return Source[^1]; return Source[lineIndex]; }
    public static string[] GetLinesFromSource(byte[] source) {
        List<string> lines = new List<string>();
        bool isString = false;
        bool isInLineComment = false;
        string line = "";
        for (int i = 0; i < source.Length; i++) {
            char Char = (char)source[i]; // ciao("ciao")
            if (isString) {
                if (Char == '\'' || Char == '\"')
                    if ((char)source[i - 1] != '\\')
                        isString = false;
                line += Char;
            } else if (isInLineComment) {
                if (Char == '\n') {
                    isInLineComment = false;
                    lines.Add(line);
                    line = "";
                }
            } else if (Char == '\n') {
                line += '\n';
                lines.Add(line);
                line = "";
            } else if (Char == SyntaxRules.InLineComment) {
                isInLineComment = true;
            } else
                line += Char;
        }
        if (!string.IsNullOrEmpty(line))
            lines.Add(line);
        return lines.ToArray();
    }
}