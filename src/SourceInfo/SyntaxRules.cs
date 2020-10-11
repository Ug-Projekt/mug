struct SyntaxRules {
    public const string True = "true";
    public const string False = "false";
    public const string IdentifierPatternChecker = "abcdefghijklmnopqrstuvwxyz_1234567890";
    public const char InLineComment = '#';
    public const string NumberPatternChecker = "01234567890";
    public static readonly string[] BuiltInKeyword = new string[] {
        "func", "pub", "self", "class", "define",
        "extern", "use", "return", "if", "elif", "else"
    };
}