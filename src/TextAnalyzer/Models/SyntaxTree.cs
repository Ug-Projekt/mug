using System;
using System.Collections.Generic;

class SyntaxTree {
    /// DEBUG METHOD
    public void PrintTree() {
        for (int i = 0; i < Count; i++)
            Console.WriteLine("Line:({2}) Token: {0}{1}", TokenType[i], (TokenValue[i] != null) ? ", "+TokenValue[i] : "", LineIndex[i]);
    }
    public SyntaxTree(List<TokenKind> tokens, List<object> values, List<short> lineIndexes) { TokenType = tokens; TokenValue = values; LineIndex = lineIndexes; }
    readonly List<TokenKind> TokenType = new List<TokenKind>();
    readonly List<object> TokenValue = new List<object>();
    readonly List<short> LineIndex = new List<short>();
    public Tuple<TokenKind, object, short> this[int index] => new Tuple<TokenKind, object, short>(TokenType[index], TokenValue[index], LineIndex[index]);
    public Tuple<TokenKind, object, short>[] this[System.Range range] {
        get {
            List<Tuple<TokenKind, object, short>> tuple = new List<Tuple<TokenKind, object, short>>();
            for (int i = 0; i < range.End.Value; i++)
                tuple.Add(new Tuple<TokenKind, object, short>(TokenType[i], TokenValue[i], LineIndex[i]));
            return tuple.ToArray();
        }
    }
    public int Count => TokenType.Count;
}