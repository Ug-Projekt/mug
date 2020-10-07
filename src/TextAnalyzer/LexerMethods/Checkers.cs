using System.Linq;
partial class Lexer {
    static bool CheckIfIsSymbol(char Char) {
        switch (Char) {
            case '(':
                InsertToken(TokenKind.SymbolOpenParenthesis);
                return true;
            case ')':
                InsertToken(TokenKind.SymbolCloseParenthesis);
                return true;
            case '[':
                InsertToken(TokenKind.SymbolOpenBracket);
                return true;
            case ']':
                InsertToken(TokenKind.SymbolCloseBracket);
                return true;
            case '{':
                InsertToken(TokenKind.SymbolOpenBrace);
                return true;
            case '}':
                InsertToken(TokenKind.SymbolCloseBrace);
                return true;
            case '=':
                if (CheckIfEqualToNext('='))
                    InsertToken(TokenKind.OperatorEqualEqual);
                else
                    InsertToken(TokenKind.SymbolEqual);
                return true;
            case '>':
                if (CheckIfEqualToNext('='))
                    InsertToken(TokenKind.OperatorMajorEqual);
                else
                    InsertToken(TokenKind.SymbolMajor);
                return true;
            case '<':
                if (CheckIfEqualToNext('='))
                    InsertToken(TokenKind.OperatorMinorEqual);
                else
                    InsertToken(TokenKind.SymbolMinor);
                return true;
            case '+':
                if (CheckIfEqualToNext('='))
                    InsertToken(TokenKind.OperatorPlusEqual);
                else
                    InsertToken(TokenKind.SymbolPlus);
                return true;
            case '-':
                if (CheckIfEqualToNext('='))
                    InsertToken(TokenKind.OperatorMinusEqual);
                else
                    InsertToken(TokenKind.SymbolMinus);
                return true;
            case '/':
                if (CheckIfEqualToNext('='))
                    InsertToken(TokenKind.OperatorSlashEqual);
                else
                    InsertToken(TokenKind.SymbolSlash);
                return true;
            case '*':
                if (CheckIfEqualToNext('='))
                    InsertToken(TokenKind.SymbolStarEqual);
                else
                    InsertToken(TokenKind.SymbolStar);
                return true;
            case '.':
                InsertToken(TokenKind.SymbolDot);
                return true;
            case ':':
                if (CheckIfEqualToNext(':'))
                    InsertToken(TokenKind.SymbolColonColon);
                else
                    InsertToken(TokenKind.SymbolColon);
                return true;
            case ';':
                InsertToken(TokenKind.ControlEndOfInstruction);
                return true;
            case ',':
                InsertToken(TokenKind.SymbolComma);
                return true;
            case '!':
                if (CheckIfEqualToNext('='))
                    InsertToken(TokenKind.OperatorNotEqual);
                else
                    InsertToken(TokenKind.SymbolNegation);
                return true;
            case '?':
                InsertToken(TokenKind.ConstNull);
                return true;
            case '\'':
            case '\"':
                if (!string.IsNullOrEmpty(Identifier) && !SyntaxRules.BuiltInKeyword.Contains(Identifier))
                    InsertIdentifierToST();
                else
                    CheckIfIsKeyword();
                if (!PassingOnString)
                    Identifier += Char;
                PassingOnString = true;
                return true;
            case '\n':
            //case '\r':
                InsertToken(TokenKind.ControlEndOfLine);
                return true;
            case '\t':
                InsertToken(TokenKind.ControlIndent);
                return true;
            case '\0':
            case '\a':
            case '\v':
            case ' ':
                return true;
            default:
                return false;
        }
    }
    static void ProcessCharType(char Char) {
        if (isIdentifierChar(Char))
            Identifier += Char;
        else {
            if (!string.IsNullOrEmpty(Identifier) && !SyntaxRules.BuiltInKeyword.Contains(Identifier))
                InsertIdentifierToST();
            else
                CheckIfIsKeyword();
            if (!CheckIfIsSymbol(Char))
                CompilationErrors.Add("Not Caratterizzable Character", $"`{Char}` is an invalid token, is not caratterizzable as Symbol, Control, Identifier", $"Remove `{Char}` from the line or replace it with the right symbol", LineIndex, CharIndex);
        }
    }
    static bool CheckIfEqualToNext(char Char) {
        if (SourceInfo.Source[LineIndex].Length - 1 < CharIndex + 1)
            return false;
        bool equal = SourceInfo.Source[LineIndex][CharIndex + 1] == Char;
        if (equal)
            Advance();
        return equal;
    }
    static void CheckIfIsKeyword() {
        for (short i = 0; i < SyntaxRules.BuiltInKeyword.Length; i++)
            if (Identifier == SyntaxRules.BuiltInKeyword[i])
                InsertKeyword((TokenKind)i);
    }
}