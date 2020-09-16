using System;
using System.Collections.Generic;
struct SourceInfo {
    public static string[] Source { get; set; }
    public static string[] GetLine(int lineIndex, short charIndex) => new string[3] { Source[lineIndex].Substring(0, charIndex), Source[lineIndex][charIndex].ToString(), Source[lineIndex].Substring(charIndex + 1) };
    public static string[] GetLinesFromSource(byte[] source) {
        List<string> lines = new List<string>();
        bool isString = false;
        string line = "";
        for (int i = 0; i < source.Length; i++)
            switch ((char)source[i]) {
                case '\r':
                case '\n':
                    line += (isString) ? "\n" : "";
                    lines.Add(line);
                    line = "";
                    break;
                case '\"':
                case '\'':
                    line += (char)source[i];
                    isString = !isString;
                    break;
                default:
                    line += (char)source[i];
                    break;
            }
        if (!string.IsNullOrEmpty(line))
            lines.Add(line);
        return lines.ToArray();
    }
}