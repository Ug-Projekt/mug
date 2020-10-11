using System.Collections.Generic;

partial class LocalParser : Parser {
    string Identifier { get; set; } = "";
    void CheckLocalParsable() {
        Objects.Clear();
        // match local scope
        // todo:
        //  - local vars
        //  - micro directives
        //  - local statements (cycles, etc...)
        if (CheckFunctionCalling())
            StoreFunctionCalling();
        else if (CheckMethodReference())
            Identifier += Current.Item2+"::";
        else if (CheckStaticReference())
            Identifier += Current.Item2 + ".";
        else
            CompilationErrors.Add("Unexpected Global Statement",
            "Cannot use this statement as valid once",
            "Rewrite the statement following language's rules", GetLineFromToken(), null);
    }
    bool CheckFunctionCalling() {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.SymbolOpenParenthesis }))
            return false;
        Identifier += Current.Item2.ToString();
        Objects.Add("func", new Data() { Name = Identifier, Type = GlobalParser.GetFunction(Current.Item2.ToString()).Data.Type });
        Objects.Add("params", new List<object>());
        Identifier = "";
        return true;
    }
    bool CheckMethodReference() {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.OperatorSelectStaticMethod }))
            return false;
        Advance();
        return true;
    }
    bool CheckStaticReference() {
        if (!CheckTokenSeries(new TokenKind[] { TokenKind.ConstIdentifier, TokenKind.SymbolDot }))
            return false;
        Advance();
        return true;
    }
}