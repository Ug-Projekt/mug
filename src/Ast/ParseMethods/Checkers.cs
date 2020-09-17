partial class Parser {
   void CheckParsable() {
      if (CheckFunction())
         StoreFunction();
      else if ()
   }
   bool CheckFunction() => CheckTokenSeries();
   bool CheckTokenSeries(TokenKind[] tokenKindsSeries) {
      var tokenSeries = _syntaxTree[TokenIndex..tokenKindsSeries.Length];
      for (int i = 0; i < tokenKindsSeries.Length; i++)
         if (tokenKindsSeries[i] != tokenSeries[i].Item1)
            return false;
      return true;
   }
   bool CheckTokenSeries(PatternMakingKind[] patterntokenSeries) {
      // make a method to transform tokenSeries[i].Item1 into patternMakingKindTokens
      var tokenKindsSeries = ;
      var tokenSeries = _syntaxTree[TokenIndex..tokenKindsSeries.Length];
      for (int i = 0; i < tokenKindsSeries.Length; i++)
         if (tokenKindsSeries[i] != tokenSeries[i].Item1)
            return false;
      return true;
   }
}