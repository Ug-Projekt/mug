using System;
using System.Collections.Generic;

class SyntaxTree {
    public SyntaxTree(List<TokenKind> tokens, List<object> values, List<short> lineIndexes) { TokenType = tokens; TokenValue = values; LineIndex = lineIndexes; }
    readonly List<TokenKind> TokenType = new List<TokenKind>();
    readonly List<object> TokenValue = new List<object>();
    readonly List<short> LineIndex = new List<short>();
    public Tuple<TokenKind, object, short> this[int index] => new Tuple<TokenKind, object, short>(TokenType[index], TokenValue[index], LineIndex[index]);
    public int Count => TokenType.Count;
}