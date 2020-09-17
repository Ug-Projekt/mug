using System.Collections.Generic;
class SyntaxTreeBuilder {
    readonly List<TokenKind> TokenType = new List<TokenKind>();
    readonly List<object> TokenValue = new List<object>();
    readonly List<short> LineIndex = new List<short>();
    public SyntaxTree Build() => new SyntaxTree(TokenType, TokenValue, LineIndex);
    public void Add(TokenKind token, object value, short lineIndex) { TokenType.Add(token); TokenValue.Add(value); LineIndex.Add(lineIndex); }
    public void Remove(int index) { TokenType.RemoveAt(index); TokenValue.RemoveAt(index); }
}