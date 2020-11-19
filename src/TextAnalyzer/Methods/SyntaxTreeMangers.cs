using System;
partial class Lexer
{
    static void InsertKeyword(TokenKind token)
    {
        Identifier = "";
        InsertToken(token);
    }
    static void InsertEndOfFileToken() => InsertToken(TokenKind.ControlEndOfFile);
    static void InsertToken(TokenKind token) => _syntaxTreeBuilder.Add(token, null, LineIndex);
    static void InsertToken(TokenKind token, object value) => _syntaxTreeBuilder.Add(token, value, LineIndex);
    static void InsertIdentifierToST()
    {
        dynamic id = Identifier;
        if (isNumber(Identifier))
        {
            long intSizeSolver = long.Parse(Identifier);
            if (intSizeSolver <= short.MaxValue && intSizeSolver >= short.MinValue)
                id = new ConstPrimitiveTInt(Convert.ToInt32(Identifier)); // fix 16 bit
            else if (intSizeSolver <= int.MaxValue && intSizeSolver >= int.MinValue)
                id = new ConstPrimitiveTInt(Convert.ToInt32(Identifier));
            else if (intSizeSolver <= long.MaxValue && intSizeSolver >= long.MinValue)
                id = new ConstPrimitiveTInt(Convert.ToInt32(Identifier)); // fix 64 bit
        }
        else if (isString(Identifier))
            id = new ConstPrimitiveTString(Identifier[1..^1]);
        else if (isBool(Identifier))
            id = new ConstPrimitiveTBool(Convert.ToBoolean(Identifier));
        else
        {
            _syntaxTreeBuilder.Add(TokenKind.Identifier, id, LineIndex);
            Identifier = "";
            return;
        }
        _syntaxTreeBuilder.Add(TokenKind.Const, id, LineIndex);
        Identifier = "";
        return;
    }
}