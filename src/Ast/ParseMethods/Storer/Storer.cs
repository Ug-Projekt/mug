using System;
partial class Parser {
   void StoreFunction(int toAdvance) {
        AstBuilder functionBodyBuilder = new AstBuilder();
        object functionIdentifier = _syntaxTree[TokenIndex + 1].Item2;
        Console.WriteLine(functionIdentifier);
        Advance(toAdvance);
        _astBuilder.Add(AstElement.Create(AstElementKind.DeclaratingFunction, functionBodyBuilder), _syntaxTree[TokenIndex].Item3);
   }
}