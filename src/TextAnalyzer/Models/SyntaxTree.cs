using System;
using System.Collections.Generic;

class SyntaxTree {
    public void PrintTree() {
        for (int i = 0; i < Count; i++)
            Console.WriteLine("Line:({2}) Token: {0}{1}", TokenType[i], !(TokenValue[i] is null) ? ", " + TokenValue[i] : "", LineIndex[i]);
    }
    public SyntaxTree(List<TokenKind> tokens, List<dynamic> values, List<short> lineIndexes) { TokenType = tokens; TokenValue = values; LineIndex = lineIndexes; }
    readonly List<TokenKind> TokenType = new List<TokenKind>();
    readonly List<dynamic> TokenValue = new List<dynamic>();
    readonly List<short> LineIndex = new List<short>();
    public Tuple<TokenKind, dynamic, short> this[int index] =>
        new Tuple<TokenKind, dynamic, short>(TokenType[index], TokenValue[index], LineIndex[index]);
    public int Count => TokenType.Count;
}