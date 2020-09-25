partial class Parser {
    bool CheckFunction(ref int i) {
        if (!CheckTokenSeries(new PatternTokensKind[] { PatternTokensKind.Type, PatternTokensKind.Id, PatternTokensKind.ParOpen }))
            return false;
        for (i = TokenIndex + 3; _syntaxTree[i].Item1 != TokenKind.SymbolCloseParenthesis;) {
            if (CheckParameter(i))
                i += 3;
            else if (CheckLastParameter(i))
                i += 2;
            else
                CompilationErrors.Add("Unexpected Token",
                "Found unexpected token in parameter declarating in function describing",
                $"Remove `{_syntaxTree[i].Item1}`: \"{_syntaxTree[i].Item2}\" and add the correct token", GetLineFromToken(), 0);
        }
        return true;
    }
    bool CheckLastParameter(int index) => CheckTokenSeries(index, new PatternTokensKind[] { PatternTokensKind.Type, PatternTokensKind.Id, PatternTokensKind.ParClose });
    bool CheckParameter(int index) => CheckTokenSeries(index, new PatternTokensKind[] { PatternTokensKind.Type, PatternTokensKind.Id, PatternTokensKind.Comma });
}