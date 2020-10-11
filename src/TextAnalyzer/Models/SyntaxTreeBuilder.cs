using System.Collections.Generic;
using System;
class SyntaxTreeBuilder {
    readonly List<TokenKind> TokenType = new List<TokenKind>();
    readonly List<object> TokenValue = new List<object>();
    readonly List<short> LineIndex = new List<short>();
    public SyntaxTree Build() => new SyntaxTree(TokenType, TokenValue, LineIndex);
    public void Add(TokenKind token, object value, short lineIndex) { TokenType.Add(token); TokenValue.Add(value); LineIndex.Add(lineIndex); }
    public void Add(Tuple<TokenKind, object, short> token) { TokenType.Add(token.Item1); TokenValue.Add(token.Item2); LineIndex.Add(token.Item3); }
    public void Remove(int index) { TokenType.RemoveAt(index); TokenValue.RemoveAt(index); }
}