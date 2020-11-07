using System;
using System.Collections.Generic;
partial class GlobalParser
{
    void CheckGlobalParsable()
    {
        Objects.Clear();
        if (CheckGlobalFunction())
            StoreGlobalFunction();
        else
            CompilationErrors.Add("Unexpected Global Statement",
            "Cannot use this statement as valid once",
            "Rewrite the statement following language's rules", GetLineFromToken(), null);
    }
    bool CheckGlobalFunction()
    {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.BuiltInKeywordFunc, TokenKind.Identifier, TokenKind.SymbolOpenParenthesis }))
            return false;
        Objects.Add("func", new Data() { Name = Next.Item2.ToString() });
        Objects.Add("params", new List<Data>());
        toAdvance = Convert.ToInt16(3 + TokenIndex);
        if (CheckTokenSeries(new TokenKind[] { TokenKind.BuiltInKeywordFunc, TokenKind.Identifier, TokenKind.SymbolOpenParenthesis, TokenKind.SymbolCloseParenthesis, TokenKind.SymbolColon }))
        {
            Objects["func"] = new Data() { Name = Objects["func"].Name, Type = _syntaxTree[toAdvance + 2].Item2 };
            toAdvance+=3;
            return true;
        }
        for (; toAdvance < _syntaxTree.Count; toAdvance++)
        {
            // intermediate param
            if (CheckTokenSeries(new TokenKind[] { TokenKind.Identifier, TokenKind.SymbolColon, TokenKind.Identifier, TokenKind.SymbolComma }, toAdvance))
            {
                Objects["params"].Add(new Data() { Name = _syntaxTree[toAdvance].Item2.ToString(), Type = _syntaxTree[toAdvance + 2].Item2 });
                toAdvance += 3;
            }
            else if (CheckTokenSeries(new TokenKind[] { TokenKind.Identifier, TokenKind.SymbolColon, TokenKind.Identifier, TokenKind.SymbolOpenBracket, TokenKind.SymbolCloseBracket, TokenKind.SymbolComma }, toAdvance))
            {
                Objects["params"].Add(new Data() { Name = _syntaxTree[toAdvance].Item2.ToString(), Type = _syntaxTree[toAdvance + 2].Item2 + "[]" });
                toAdvance += 5;
            }
            else if (CheckTokenSeries(new TokenKind[] { TokenKind.Identifier, TokenKind.SymbolColon, TokenKind.Identifier, TokenKind.SymbolMinor, TokenKind.Identifier, TokenKind.SymbolMajor, TokenKind.SymbolComma }, toAdvance))
            {
                Objects["params"].Add(new Data() { Name = _syntaxTree[toAdvance].Item2.ToString(), Type = _syntaxTree[toAdvance + 2].Item2 + "<" + _syntaxTree[toAdvance + 4].Item2 + ">" });
                toAdvance += 6;
            }
            // last param
            else if (CheckTokenSeries(new TokenKind[] { TokenKind.Identifier, TokenKind.SymbolColon, TokenKind.Identifier, TokenKind.SymbolCloseParenthesis }, toAdvance))
            {
                Objects["params"].Add(new Data() { Name = _syntaxTree[toAdvance].Item2.ToString(), Type = _syntaxTree[toAdvance + 2].Item2 });
                toAdvance += 3;
                break;
            }
            else if (CheckTokenSeries(new TokenKind[] { TokenKind.Identifier, TokenKind.SymbolColon, TokenKind.Identifier, TokenKind.SymbolOpenBracket, TokenKind.SymbolCloseBracket, TokenKind.SymbolCloseParenthesis }, toAdvance))
            {
                Objects["params"].Add(new Data() { Name = _syntaxTree[toAdvance].Item2.ToString(), Type = _syntaxTree[toAdvance + 2].Item2 + "[]" });
                toAdvance += 5;
                break;
            }
            else if (CheckTokenSeries(new TokenKind[] { TokenKind.Identifier, TokenKind.SymbolColon, TokenKind.Identifier, TokenKind.SymbolMinor, TokenKind.Identifier, TokenKind.SymbolMajor, TokenKind.SymbolCloseParenthesis }, toAdvance))
            {
                Objects["params"].Add(new Data() { Name = _syntaxTree[toAdvance].Item2.ToString(), Type = _syntaxTree[toAdvance + 2].Item2 + "<" + _syntaxTree[toAdvance + 4].Item2 + ">" });
                toAdvance += 6;
                break;
            }
            else
            {
                CompilationErrors.Add("Unexpected Token In Parameter List",
                "Cannot find a right token statement in function's parameters declaration",
                "Use this token pattern: `func <id>(p1, p2, ...): <t>`", GetLineFromToken(), null);
                return false;
            }
        }
        Objects["func"] = new Data() { Name = Objects["func"].Name, Type = _syntaxTree[toAdvance + 2].Item2 };
        toAdvance += 3;
        return true;
    }
}