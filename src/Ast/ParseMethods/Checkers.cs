using System;
partial class Parser {
   void CheckParsable() {
      CheckFunction();
      //if (CheckFunction())
        // StoreFunction();
      //else
        // CompilationErrors.Add("Unexpected Tokens",
         //"A token series in of this statement is not caratterizable as: `Statement`, `Function`, etc..",
         //"Fix the statement with the corrects tokens", GetLineFromToken(), 0);
   }
   bool CheckFunction() {
      if (!CheckTokenSeries(new PatternTokensKind[] { PatternTokensKind.Type, PatternTokensKind.Id, PatternTokensKind.ParOpen }))
         return false;
      for (int i = TokenIndex+3; _syntaxTree[i].Item1 != TokenKind.SymbolCloseParenthesis; i++) {
         if (CheckParameters())
            Console.WriteLine("LastParam");
         else if (CheckLastParameters())
            Console.WriteLine("LastParam");
         else
            CompilationErrors.Add("Unexpected Token",
            "Found unexpected token in parameter declarating in function describing",
            $"Remove `{_syntaxTree[i].Item1}: {_syntaxTree[i].Item2} and add the correct token`", GetLineFromToken(), 0);
      }
      return true;
   }
   bool CheckLastParameters() => CheckTokenSeries(new PatternTokensKind[] { PatternTokensKind.Type, PatternTokensKind.Id, PatternTokensKind.ParClose });
   bool CheckParameters() => CheckTokenSeries(new PatternTokensKind[] { PatternTokensKind.Type, PatternTokensKind.Id, PatternTokensKind.Comma  });
   bool CheckTokenSeries(TokenKind[] tokenKindsSeries) {
      var codeTokens = _syntaxTree[TokenIndex..tokenKindsSeries.Length];
      for (int i = 0; i < tokenKindsSeries.Length; i++)
         if (tokenKindsSeries[i] != codeTokens[i].Item1)
            return false;
      return true;
   }
   bool CheckTokenSeries(PatternTokensKind[] pattTokSeries) {
      var codeTokens = Schema.Parse(_syntaxTree.GetTokensFromRange(TokenIndex..pattTokSeries.Length));
      for (int i = 0; i < pattTokSeries.Length; i++)
         if (pattTokSeries[i] != codeTokens[i])
            return false;
      return true;
   }
}