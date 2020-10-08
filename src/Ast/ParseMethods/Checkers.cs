using System;
using System.Collections.Generic;
partial class Parser {
    void CheckParsable() {
        Objects.Clear();
        // match global scope
        // todo:
        //  - global vars
        //  - global funcs (main, etc...)
        //  - classes
        //  - directives
        //  - function properties
        if (CheckFunction())
            StoreFunction();
        else
            CompilationErrors.Add("Unexpected Statement",
            "Cannot find a statement with these tokens series",
            "Fix the statement with the right tokens", GetLineFromToken(), null);
    }
    bool CheckFunction() {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.BuiltInKeywordFunc, TokenKind.ConstIdentifier, TokenKind.SymbolOpenParenthesis }))
            return false;
        Objects.Add(Next.Item2);
        toAdvance = Convert.ToInt16(3+TokenIndex);
        if (CheckTokenSeries(new TokenKind[] { TokenKind.BuiltInKeywordFunc, TokenKind.ConstIdentifier, TokenKind.SymbolOpenParenthesis, TokenKind.SymbolCloseParenthesis })) {
            toAdvance++;
            return true;
        }
        for (; toAdvance<_syntaxTree.Count;toAdvance++) {
            Objects.Add(_syntaxTree[toAdvance].Item2);
            // Console.WriteLine(_syntaxTree[i+TokenIndex]);
            // intermediate param
            if (CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.SymbolComma }, toAdvance))
                toAdvance++;
            // last param
            else if (CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.SymbolCloseParenthesis }, toAdvance))
                break;
            else {
                CompilationErrors.Add("Unexpected Token In Parameter List",
                "Cannot find a right token statement in function's parameters declaration",
                "Use this token pattern: `func <id>(p1, p2, ...)`", GetLineFromToken(), null);
                return false;
            }
        }
        toAdvance+=1;
        return true;
    }
    bool CheckTokenSeries(TokenKind[] pattTokSeries, int tokenIndex = -1) {
        if (tokenIndex == -1)
            tokenIndex = TokenIndex;
        if (_syntaxTree.Count <= pattTokSeries.Length+TokenIndex)
            return false;
        for (int i=0;i<pattTokSeries.Length;i++) {
            // Console.WriteLine(pattTokSeries[i]+" "+_syntaxTree[i].Item1);
            if (pattTokSeries[i] != _syntaxTree[i+tokenIndex].Item1)
                return false;
        }
        return true;
    }
}