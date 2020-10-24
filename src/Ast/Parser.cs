using System;
using System.Collections.Generic;
abstract class Parser
{
    virtual public AstBuilder _astBuilder { get; set; } = new AstBuilder();
    virtual public SyntaxTree _syntaxTree { get; set; }
    virtual public Tuple<TokenKind, dynamic, short> Current => (TokenIndex >= _syntaxTree.Count) ? new Tuple<TokenKind, dynamic, short>(TokenKind.ControlEndOfFile, null, 0) : _syntaxTree[TokenIndex];
    virtual public Tuple<TokenKind, dynamic, short> Next => (TokenIndex + 1 >= _syntaxTree.Count) ? new Tuple<TokenKind, dynamic, short>(TokenKind.ControlEndOfFile, null, 0) : _syntaxTree[TokenIndex + 1];
    virtual public short toAdvance { get; set; } = 0;
    virtual public int TokenIndex { get; set; }
    virtual public Dictionary<string, dynamic> Objects { get; set; } = new Dictionary<string, dynamic>();
    abstract public Ast GetAbstractSyntaxTree(SyntaxTree synT);
    virtual public short GetLineFromToken() => _syntaxTree[TokenIndex].Item3;
    virtual public void Advance() => TokenIndex++;
    virtual public void Advance(short count) => TokenIndex += count;
    virtual public bool CheckTokenSeries(TokenKind[] pattTokSeries, int tokenIndex = -1)
    {
        if (tokenIndex == -1)
            tokenIndex = TokenIndex;
        if (_syntaxTree.Count <= pattTokSeries.Length + TokenIndex)
            return false;
        for (int i = 0; i < pattTokSeries.Length; i++)
        {
            // Console.WriteLine(pattTokSeries[i]+" "+_syntaxTree[i].Item1);
            if (pattTokSeries[i] != _syntaxTree[i + tokenIndex].Item1)
                return false;
        }
        return true;
    }
}