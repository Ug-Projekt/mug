// using System.Collections.Generic;
// class Patterns {
//     static Dictionary<TokenKind, PatternTokensKind> MakePatternFromTokens = new Dictionary<TokenKind, PatternTokensKind>()
//     {
//       {TokenKind.BuiltInKeywordInt16, PatternTokensKind.ValueType},
//       {TokenKind.BuiltInKeywordInt32, PatternTokensKind.ValueType},
//       {TokenKind.BuiltInKeywordInt64, PatternTokensKind.ValueType},
//       {TokenKind.BuiltInKeywordString, PatternTokensKind.ValueType},
//       {TokenKind.BuiltInKeywordChar, PatternTokensKind.ValueType},
//       {TokenKind.BuiltInKeywordImplicitType, PatternTokensKind.ValueType},
//       {TokenKind.BuiltInKeywordVoid, PatternTokensKind.ValueType},
//       {TokenKind.BuiltInKeywordIf, PatternTokensKind.Condition},
//       {TokenKind.BuiltInKeywordElif, PatternTokensKind.Condition},
//       {TokenKind.BuiltInKeywordElse, PatternTokensKind.Condition},
//       {TokenKind.ConstBool, PatternTokensKind.Const},
//       {TokenKind.ConstChar, PatternTokensKind.Const},
//       {TokenKind.ConstFloat16, PatternTokensKind.Const},
//       {TokenKind.ConstFloat, PatternTokensKind.Const},
//       {TokenKind.ConstFloat64, PatternTokensKind.Const},
//       {TokenKind.ConstInt16, PatternTokensKind.Const},
//       {TokenKind.ConstInt32, PatternTokensKind.Const},
//       {TokenKind.ConstInt64, PatternTokensKind.Const},
//       {TokenKind.ConstString, PatternTokensKind.Const},
//       {TokenKind.ConstIdentifier, PatternTokensKind.Id},
//       {TokenKind.SymbolOpenParenthesis, PatternTokensKind.ParOpen},
//       {TokenKind.SymbolCloseParenthesis, PatternTokensKind.ParClose},
//       {TokenKind.SymbolOpenBracket, PatternTokensKind.BrackOpen},
//       {TokenKind.SymbolCloseBracket, PatternTokensKind.BrackClose},
//       {TokenKind.SymbolOpenBrace, PatternTokensKind.BraceClose},
//       {TokenKind.SymbolCloseBrace, PatternTokensKind.BraceClose},
//       {TokenKind.SymbolComma, PatternTokensKind.Comma},
//       {TokenKind.ControlEndOfFile, PatternTokensKind.Eof},
//       {TokenKind.ControlEndOfInstruction, PatternTokensKind.SemiColon},
//    };
//     public static PatternTokensKind[] Parse(TokenKind[] tokens) {
//         var result = new List<PatternTokensKind>();
//         for (int i = 0; i < tokens.Length; i++)
//             result.Add(MakePatternFromTokens[tokens[i]]);
//         return result.ToArray();
//     }
// }