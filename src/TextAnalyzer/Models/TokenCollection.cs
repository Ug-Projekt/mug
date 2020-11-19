using System;
using System.Collections.Generic;

class TokenCollection
{
    public void PrintTree()
    {
        for (int i = 0; i < Count; i++)
            Console.WriteLine("Line:({2}) Token: {0}{1}", TokenType[i], !(TokenValue[i] is null) ? ", " + TokenValue[i] : "", LineIndex[i]);
    }
    public TokenCollection(List<TokenKind> tokens, List<dynamic> values, List<short> lineIndexes) { TokenType = tokens; TokenValue = values; LineIndex = lineIndexes; }
    readonly List<TokenKind> TokenType = new List<TokenKind>();
    readonly List<dynamic> TokenValue = new List<dynamic>();
    readonly List<short> LineIndex = new List<short>();
    public Tuple<TokenKind, dynamic, short> this[int index] =>
        new Tuple<TokenKind, dynamic, short>(TokenType[index], TokenValue[index], LineIndex[index]);
    public override string ToString()
    {
        string tree = "";
        const string e = "";
        for (int i = 0; i < Count; i++)
            tree += $"Line:({LineIndex[i]}) Token: {TokenType[i]}{(!(TokenValue[i] is null) ? ", " + TokenValue[i] : e)}\n\t";
        return tree;
    }
    public int Count => TokenType.Count;
}