partial class Lexer {
    static void InsertKeyword(TokenKind token) {
        Identifier = "";
        InsertToken(token);
    }
    static void InsertEndOfFileToken() => InsertToken(TokenKind.ControlEndOfFile);
    static void InsertToken(TokenKind token) => _syntaxTreeBuilder.Add(token, null, LineIndex);
    static void InsertToken(TokenKind token, object value) => _syntaxTreeBuilder.Add(token, value, LineIndex);
    static void InsertIdentifierToST() {
        TokenKind token = TokenKind.ConstIdentifier;
        if (isNumber(Identifier)) {
            long intSizeSolver = long.Parse(Identifier);
            if (intSizeSolver <= short.MaxValue && intSizeSolver >= short.MinValue)
                token = TokenKind.ConstInt16;
            else if (intSizeSolver <= int.MaxValue && intSizeSolver >= int.MinValue)
                token = TokenKind.ConstInt32;
            else if (intSizeSolver <= long.MaxValue && intSizeSolver >= long.MinValue)
                token = TokenKind.ConstInt64;
        } else if (isString(Identifier))
            token = TokenKind.ConstString;
        else if (isChar(Identifier))
            token = TokenKind.ConstChar;
        else if (isBool(Identifier))
            token = TokenKind.ConstBool;
        _syntaxTreeBuilder.Add(token, Identifier, LineIndex);
        Identifier = "";
    }
}