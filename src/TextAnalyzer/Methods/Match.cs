using System.Linq;
partial class Lexer
{
    static bool MatchSymbols(char Char)
    {
        switch (Char)
        {
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
                if (MatchNext('='))
                    InsertToken(TokenKind.OperatorEqualEqual);
                else
                    InsertToken(TokenKind.SymbolEqual);
                return true;
            case '>':
                if (MatchNext('='))
                    InsertToken(TokenKind.OperatorMajorEqual);
                else
                    InsertToken(TokenKind.SymbolMajor);
                return true;
            case '<':
                if (MatchNext('='))
                    InsertToken(TokenKind.OperatorMinorEqual);
                else
                    InsertToken(TokenKind.SymbolMinor);
                return true;
            case '+':
                if (MatchNext('='))
                    InsertToken(TokenKind.OperatorPlusEqual);
                else
                    InsertToken(TokenKind.SymbolPlus);
                return true;
            case '-':
                if (MatchNext('='))
                    InsertToken(TokenKind.OperatorMinusEqual);
                else
                    InsertToken(TokenKind.SymbolMinus);
                return true;
            case '/':
                if (MatchNext('='))
                    InsertToken(TokenKind.OperatorSlashEqual);
                else
                    InsertToken(TokenKind.SymbolSlash);
                return true;
            case '*':
                if (MatchNext('='))
                    InsertToken(TokenKind.SymbolStarEqual);
                else
                    InsertToken(TokenKind.SymbolStar);
                return true;
            case '.':
                InsertToken(TokenKind.SymbolDot);
                return true;
            case ':':
                if (MatchNext(':'))
                    InsertToken(TokenKind.OperatorSelectStaticMethod);
                else
                    InsertToken(TokenKind.SymbolColon);
                return true;
            case ';':
                InsertToken(TokenKind.SymbolSemiColon);
                return true;
            case ',':
                InsertToken(TokenKind.SymbolComma);
                return true;
            case '!':
                if (MatchNext('='))
                    InsertToken(TokenKind.OperatorNotEqual);
                else
                    InsertToken(TokenKind.SymbolNegation);
                return true;
            case '?':
                InsertToken(TokenKind.Identifier, '?');
                return true;
            case '\"':
                if (!string.IsNullOrEmpty(Identifier) && !SyntaxRules.BuiltInKeyword.Contains(Identifier))
                    InsertIdentifierToST();
                else
                    MatchKeyword();
                if (!PassingOnString)
                    Identifier += Char;
                PassingOnString = true;
                return true;
            case '\n':
            case '\r':
            case '\t':
            case '\0':
            case '\a':
            case '\v':
            case ' ':
                return true;
            default:
                return false;
        }
    }
    static void ProcessCharType(char Char)
    {
        if (isIdentifierChar(Char))
            Identifier += Char;
        else
        {
            if (!string.IsNullOrEmpty(Identifier) && !SyntaxRules.BuiltInKeyword.Contains(Identifier))
                InsertIdentifierToST();
            else
                MatchKeyword();
            if (!MatchSymbols(Char))
                CompilationErrors.Add("Not Caratterizzable Character", $"`{Char}` is an invalid token, is not caratterizzable as Symbol, Control, Identifier", $"Remove `{Char}` from the line or replace it with the right symbol", LineIndex, CharIndex);
        }
    }
    static bool MatchNext(char Char)
    {
        if (SourceInfo.Source[LineIndex].Length - 1 < CharIndex + 1)
            return false;
        bool equal = SourceInfo.Source[LineIndex][CharIndex + 1] == Char;
        if (equal)
            Advance();
        return equal;
    }
    static bool MatchNext(char Char, short count)
    {
        if (SourceInfo.Source[LineIndex].Length - 1 < CharIndex + count)
            return false;
        bool equal = false;
        for (int i = 0; i < count; i++)
        {
            equal = SourceInfo.Source[LineIndex][CharIndex + i] == Char;
            if (!equal)
                return false;
        }
        if (equal)
            Advance(count);
        return equal;
    }
    static void MatchKeyword()
    {
        for (short i = 0; i < SyntaxRules.BuiltInKeyword.Length; i++)
            if (Identifier == SyntaxRules.BuiltInKeyword[i])
                InsertKeyword((TokenKind)i);
    }
}