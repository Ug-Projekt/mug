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
        Objects.Add("params", new List<object>());
        AstBuilder paramBuilder = new AstBuilder();
        for (toAdvance = Convert.ToInt16(1 + TokenIndex); ; toAdvance++)
        {
            //Console.WriteLine("-> "+_syntaxTree[toAdvance]);
            if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolCloseParenthesis && isRelativeBrace == 1)
                break;
            else if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolOpenParenthesis)
                isRelativeBrace++;
            else if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolCloseParenthesis)
                isRelativeBrace--;
            else if (_syntaxTree[toAdvance].Item1 == TokenKind.SymbolComma && isRelativeBrace == 1)
            {
                Objects["params"].Add(paramBuilder.Build());
                paramBuilder.Clear();
            }
            else
                paramBuilder.Add(AstElement.New(
                    AstElementKind.PassingValueInFCStatement,
                    null, _syntaxTree[toAdvance].Item2, _syntaxTree[toAdvance].Item1), GetLineFromToken());
        }
        if (!paramBuilder.IsEmpty())
            Objects["params"].Add(paramBuilder.Build());
        Identifier = "";
        return true;
    }
}