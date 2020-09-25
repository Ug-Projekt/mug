using System;
partial class Parser {
    void CheckParsable() {
        int toAdvance = 0;
        if (CheckFunction(ref toAdvance))
            StoreFunction(toAdvance);
        else
            CompilationErrors.Add("Unexpected Tokens",
                "A token series in of this statement is not caratterizable as: `Statement`, `Function`, etc..",
                "Fix the statement with the corrects tokens", GetLineFromToken(), 0);
    }
    bool CheckTokenSeries(int index, PatternTokensKind[] pattTokSeries) {
        for (short i = 0; i < pattTokSeries.Length; i++)
            if (TokensKindPatterns.Parse[_syntaxTree[index + i].Item1] != pattTokSeries[i])
                return false;
        return true;
    }
    bool CheckTokenSeries(PatternTokensKind[] pattTokSeries) {
        for (short i = 0; i < pattTokSeries.Length; i++) {
            if (TokensKindPatterns.Parse[_syntaxTree[TokenIndex + i].Item1] != pattTokSeries[i])
                return false;
        }
        return true;
    }
}