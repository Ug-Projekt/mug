using System;
using System.Collections.Immutable;

struct SyntaxRules {
    public const string True = "true";
    public const string False = "false";
    public const string IdentifierPatternChecker = "abcdefghijklmnopqrstuvwxyz_1234567890";
    public const char InLineComment = '#';
    public const string NumberPatternChecker = "01234567890";
    public static readonly string[] BuiltInKeyword = new string[] { "int16", "int", "int64", "string", "char", "return", "if", "elif", "else", "new", "type" };
}