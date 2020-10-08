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
        CompilationErrors.Except(true);
        return _astBuilder.Build();
    }
    Ast GetBody(short min_indent) {
        var stb = new SyntaxTreeBuilder();
        while (Current.Item1 != TokenKind.ControlEndOfFile) {
            if (Current.Item1 == TokenKind.ControlIndent && Convert.ToInt16(Current.Item2) < min_indent)
                break;
            stb.Add(Current.Item1, Current.Item2, Current.Item3);
            Advance();
        }
        Console.WriteLine("Body:");
        stb.Build().PrintTree();
        Console.WriteLine("End Body");
        Advance(Convert.ToInt16(stb.Build().Count));
        //return new AstBuilder().Build();
        return new Parser().GetAbstractSyntaxTree(stb.Build());
    }
    short GetLineFromToken() => _syntaxTree[TokenIndex].Item3;
    void Advance() => TokenIndex++;
    void Advance(short count) => TokenIndex += count;
}