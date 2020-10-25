using System;
using System.Collections.Generic;

partial class LocalParser : Parser
{
    string Identifier { get; set; } = "";
    void CheckLocalParsable()
    {
        Objects.Clear();
        if (CheckFunctionCalling())
            StoreFunctionCalling();
        else
            CompilationErrors.Add("Unexpected Local Statement",
            "Cannot use this statement as valid once",
            "Rewrite the statement following language's rules", GetLineFromToken(), null);
    }
    bool CheckFunctionCalling()
    {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.SymbolOpenParenthesis }))
            return false;
        int isRelativeBrace = 0;
        Identifier += Current.Item2.ToString();
        Objects.Add("func", new Data() { Name = Identifier });
        var exprBuilder = new SyntaxTreeBuilder();
        var paramBuilder = new AstBuilder();
        for (toAdvance = Convert.ToInt16(1 + TokenIndex); ; toAdvance++)
        {
            if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolCloseParenthesis && isRelativeBrace == 1)
                break;
            else if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolOpenParenthesis)
                isRelativeBrace++;
            else if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolCloseParenthesis)
                isRelativeBrace--;
            else if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolComma && isRelativeBrace == 1)
            {
                paramBuilder.Add(
                    AstElement.New(
                        AstElementKind.Expression,
                        null, exprBuilder.Build()
                    ),
                    GetLineFromToken()
                );
                exprBuilder = new SyntaxTreeBuilder();
            }
            else
                exprBuilder.Add(_syntaxTree[toAdvance]);
        }
        if (exprBuilder.Count > 0)
            paramBuilder.Add(
                AstElement.New(
                    AstElementKind.Expression,
                    null, exprBuilder.Build()
                ),
                GetLineFromToken()
            );

        Identifier = "";
        Objects.Add("params", paramBuilder.Build());
        return true;
    }
}