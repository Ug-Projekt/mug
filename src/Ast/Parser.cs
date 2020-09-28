partial class Parser {
   AstBuilder _astBuilder { get; set; } = new AstBuilder();
   SyntaxTree _syntaxTree;
   short toAdvance {get;set;} = 0;
   int TokenIndex { get; set; }
   public Ast GetAbstractSyntaxTree(SyntaxTree synT) {
      _syntaxTree = synT;
      while (synT[TokenIndex].Item1 != TokenKind.ControlEndOfFile) {
         CheckParsable();
         Advance();
      }
      return _astBuilder.Build();
   }
   short GetLineFromToken() => _syntaxTree[TokenIndex].Item3;
   void Advance() => TokenIndex++;
   void Advance(short count) => TokenIndex+=count;
}