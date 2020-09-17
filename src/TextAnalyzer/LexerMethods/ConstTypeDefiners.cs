partial class Lexer {
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
}