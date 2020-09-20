using System.Collections.Generic;
class Schema {
   static Dictionary<TokenKind, PatternTokensKind> ParseSchema = new Dictionary<TokenKind, PatternTokensKind>()
   {
      {TokenKind.BuiltInKeywordInt16, PatternTokensKind.Type},
      {TokenKind.BuiltInKeywordInt32, PatternTokensKind.Type},
      {TokenKind.BuiltInKeywordInt64, PatternTokensKind.Type},
      {TokenKind.BuiltInKeywordString, PatternTokensKind.Type},
      {TokenKind.BuiltInKeywordChar, PatternTokensKind.Type},
      {TokenKind.BuiltInKeywordIf, PatternTokensKind.Condition},
      {TokenKind.BuiltInKeywordElif, PatternTokensKind.Condition},
      {TokenKind.BuiltInKeywordElse, PatternTokensKind.Condition},
      {TokenKind.ConstBool, PatternTokensKind.Value},
      {TokenKind.ConstChar, PatternTokensKind.Value},
      {TokenKind.ConstFloat16, PatternTokensKind.Value},
      {TokenKind.ConstFloat, PatternTokensKind.Value},
      {TokenKind.ConstFloat64, PatternTokensKind.Value},
      {TokenKind.ConstInt16, PatternTokensKind.Value},
      {TokenKind.ConstInt32, PatternTokensKind.Value},
      {TokenKind.ConstInt64, PatternTokensKind.Value},
      {TokenKind.ConstString, PatternTokensKind.Value},
      {TokenKind.ConstIdentifier, PatternTokensKind.Id},
      {TokenKind.SymbolOpenParenthesis, PatternTokensKind.ParOpen},
      {TokenKind.SymbolCloseParenthesis, PatternTokensKind.ParClose},
      {TokenKind.SymbolOpenBracket, PatternTokensKind.BrackOpen},
      {TokenKind.SymbolCloseBracket, PatternTokensKind.BrackClose},
      {TokenKind.SymbolOpenBrace, PatternTokensKind.BraceClose},
      {TokenKind.SymbolCloseBrace, PatternTokensKind.BraceClose},
      {TokenKind.SymbolComma, PatternTokensKind.Comma},
   };
   public static PatternTokensKind[] Parse(TokenKind[] tokens) {
      var result = new List<PatternTokensKind>();
      for (int i = 0; i < tokens.Length; i++)
         result.Add(ParseSchema[tokens[i]]);
      return result.ToArray();
   }
}