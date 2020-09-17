partial class Parser {
   AstBuilder _astBuilder { get; set; }
   SyntaxTree _syntaxTree;
   int TokenIndex { get; set; }
   public Ast GetAbstractSyntaxTree(SyntaxTree synT) {
      _syntaxTree = synT;
      while (synT[TokenIndex].Item1 != TokenKind.ControlEndOfFile) {
         CheckParsable();
         Advance();
      }
      return _astBuilder.Build();
   }
   void Advance() => TokenIndex++;
}