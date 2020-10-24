using System.Collections.Generic;
using System;
class SyntaxTreeBuilder {
    readonly List<TokenKind> TokenType = new List<TokenKind>();
    readonly List<dynamic> TokenValue = new List<dynamic>();
    readonly List<short> LineIndex = new List<short>();
    public SyntaxTree Build() => new SyntaxTree(TokenType, TokenValue, LineIndex);
    public void Add(TokenKind token, dynamic value, short lineIndex) { TokenType.Add(token); TokenValue.Add(value); LineIndex.Add(lineIndex); }
    public void Add(Tuple<TokenKind, dynamic, short> token) { TokenType.Add(token.Item1); TokenValue.Add(token.Item2); LineIndex.Add(token.Item3); }
    public void Remove(int index) { TokenType.RemoveAt(index); TokenValue.RemoveAt(index); }
}