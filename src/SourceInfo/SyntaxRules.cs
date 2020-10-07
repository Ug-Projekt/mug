struct SyntaxRules {
    public const string True = "true";
    public const string False = "false";
    public const string IdentifierPatternChecker = "abcdefghijklmnopqrstuvwxyz_1234567890";
    public const char InLineComment = '#';
    public const string NumberPatternChecker = "01234567890";
    public static readonly string[] BuiltInKeyword = new string[] {
        "var", "func", "try", "catch", "pub", "self", "class", "field",
        "define", "extern", "use", "return", "if", "elif", "else"
        };
}