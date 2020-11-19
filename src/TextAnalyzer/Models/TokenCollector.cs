using System;
using System.Collections.Generic;
class TokenCollector
{
    readonly List<TokenKind> TokenType = new List<TokenKind>();
    readonly List<dynamic> TokenValue = new List<dynamic>();
    readonly List<short> LineIndex = new List<short>();
    public TokenCollection Build() => new TokenCollection(TokenType, TokenValue, LineIndex);
    public void Add(TokenKind token, dynamic value, short lineIndex) { TokenType.Add(token); TokenValue.Add(value); LineIndex.Add(lineIndex); }
    public void Add(Tuple<TokenKind, dynamic, short> token) { TokenType.Add(token.Item1); TokenValue.Add(token.Item2); LineIndex.Add(token.Item3); }
    public void Remove(int index, int count = 1) { TokenType.RemoveRange(index, count); TokenValue.RemoveRange(index, count); LineIndex.RemoveRange(index, count); }
    public TokenCollection NormalizeAndBuild()
    {
        for (int i = 0; i < Count; i++)
            if (TokenType[i] == TokenKind.SymbolDot || TokenType[i] == TokenKind.OperatorSelectStaticMethod)
            {
                if (TokenType[i - 1] == TokenKind.Identifier && TokenType[i + 1] == TokenKind.Identifier)
                {
                    TokenValue[i - 1] += (TokenType[i] == TokenKind.OperatorSelectStaticMethod ? "::" : ".") + TokenValue[i + 1];
                    Remove(i, 2);
                    i-=2;
                }
                else
                    CompilationErrors.Add(
                        "Expected Identifier",
                        "Before and after `.` or `::` operators there must be an identifier",
                        "Remove the operator or add the missing identifier", LineIndex[i], null);
            }
        return Build();
    }
    public int Count => TokenType.Count;
}