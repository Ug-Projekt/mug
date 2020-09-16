using System;
using System.Collections.Generic;
using System.Linq;

class Lexer {
    static bool PassingOnString { get; set; }
    static short CharIndex { get; set; }
    static short LineIndex { get; set; }
    static string Identifier { get; set; }
    static SyntaxTreeBuilder _syntaxTreeBuilder { get; set; }
    public static SyntaxTree GetSyntaxTree(byte[] source) {
        initializeComponents(source);

        while (LineIndex < SourceInfo.Source.Length) {
            while (CharIndex < SourceInfo.Source[LineIndex].Length) {
                if (PassingOnString) {
                    Identifier += SourceInfo.Source[LineIndex][CharIndex];
                    if (SourceInfo.Source[LineIndex][CharIndex] == '\'' || SourceInfo.Source[LineIndex][CharIndex] == '\"') {
                        if (!string.IsNullOrEmpty(Identifier) && !SyntaxRules.BuiltInKeyword.Contains(Identifier))
                            InsertIdentifierToST();
                        else
                            CheckIfIsKeyword();
                        PassingOnString = false;
                    }
                } else
                    CheckCharType(SourceInfo.Source[LineIndex][CharIndex]);
                Advance();
            }
            AdvanceLine();
        }
        InsertEndOfFileToken();
        return _syntaxTreeBuilder.Build();
    }
    static bool isIdentifierChar(char Char) => SyntaxRules.IdentifierPatternChecker.Contains(char.ToLower(Char));
    static bool isNumber(string String) {
        for (int i = 0; i < String.Length; i++)
            if (!SyntaxRules.NumberPatternChecker.Contains(String[i]))
                return false;
        return true;
    }
    static bool isString(string String) => String[0] == '\"' && String[^1] == '\"';
    static bool isChar(string String) => String[0] == '\'' && String[^1] == '\'';
    static bool isBool(string String) => String == SyntaxRules.True || String == SyntaxRules.False;
    static void CheckCharType(char Char) {
        if (isIdentifierChar(Char))
            Identifier += Char;
        else {
            if (!string.IsNullOrEmpty(Identifier) && !SyntaxRules.BuiltInKeyword.Contains(Identifier))
                InsertIdentifierToST();
            else
                CheckIfIsKeyword();
            if (char.IsControl(Char))
                return;
            else if (!CheckIfIsSymbol(Char))
                CompilationErrors.Add("Not Caratterizzable Character", $"`{Char}` is an invalid token, is not caratterizzable as Symbol, Control, Identifier", $"Remove `{Char}` from the line or replace it with the right symbol", LineIndex, CharIndex);
        }
    }
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
            case ' ':
                return true;
            default:
                return false;
        }
    }
    static bool CheckIfEqualToNext(char Char) {
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
    static void InsertKeyword(TokenKind token) {
        Identifier = "";
        InsertToken(token);
    }
    static void InsertEndOfFileToken() => InsertToken(TokenKind.ControlEndOfFile);
    static void InsertToken(TokenKind token) => _syntaxTreeBuilder.Add(token, null, LineIndex);
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
    static void AdvanceLine() => LineIndex++;
    static void AdvanceLine(short count) => LineIndex += count;
    static void Advance() => CharIndex++;
    static void Advance(short count) => CharIndex += count;
    static void initializeComponents(byte[] source) {
        _syntaxTreeBuilder = new SyntaxTreeBuilder();
        SourceInfo.Source = SourceInfo.GetLinesFromSource(source);
        CharIndex = 0;
        LineIndex = 0;
        Identifier = "";
        PassingOnString = false;
    }
    public static int GetLineIndex() => LineIndex;
    public static int GetCharIndex() => CharIndex;
}