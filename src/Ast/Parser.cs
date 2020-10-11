using System;
using System.Collections.Generic;
partial class Parser {
    AstBuilder _astBuilder { get; set; } = new AstBuilder();
    SyntaxTree _syntaxTree;
    Tuple<TokenKind, object, short> Current => _syntaxTree[TokenIndex];
    Tuple<TokenKind, object, short> Next => _syntaxTree[TokenIndex + 1];
    short toAdvance { get; set; } = 0;
    int TokenIndex { get; set; }
    List<object> Objects { get; set; } = new List<object>();
    public Ast GetAbstractSyntaxTree(SyntaxTree synT) {
        _syntaxTree = synT;
        while (Current.Item1 != TokenKind.ControlEndOfFile) {
            CheckParsable();
            Advance();
        }
        return _astBuilder.Build();
    }
    short GetLineFromToken() => _syntaxTree[TokenIndex].Item3;
    void Advance() => TokenIndex++;
    void Advance(short count) => TokenIndex += count;
}