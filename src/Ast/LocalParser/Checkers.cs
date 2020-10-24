using System;
using System.Collections.Generic;

partial class LocalParser : Parser {
    string Identifier { get; set; } = "";
    void CheckLocalParsable() {
        Objects.Clear();
        if (CheckFunctionCalling())
            StoreFunctionCalling();
        else
            CompilationErrors.Add("Unexpected Global Statement",
            "Cannot use this statement as valid once",
            "Rewrite the statement following language's rules", GetLineFromToken(), null);
    }
    bool CheckFunctionCalling() {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.SymbolOpenParenthesis }))
            return false;
        int isRelativeBrace = 0;
        Identifier += Current.Item2.ToString();
        Objects.Add("func", new Data() { Name = Identifier });
        Objects.Add("params", new List<object>());
        for (toAdvance = Convert.ToInt16(1+TokenIndex); ; toAdvance++) {
            Console.WriteLine("-> "+_syntaxTree[toAdvance]);
            if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolCloseParenthesis && isRelativeBrace == 0)
                break;
            else if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolOpenParenthesis)
                isRelativeBrace++;
            else if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolCloseParenthesis)
                isRelativeBrace--;
            else
                Objects["params"].Add(_syntaxTree[toAdvance].Item2);
        }
        Identifier = "";
        return true;
    }
    bool CheckStaticElement() {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.OperatorSelectStaticMethod }))
            return false;
        Advance();
        return true;
    }
    bool CheckInstanceElement() {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.SymbolDot }))
            return false;
        Advance();
        return true;
    }
}