using System;
using System.Collections.Generic;
class Ast {
   public Ast(AstElement[] ElementKind, int[] LineIndex) {
      this.ElementKind = ElementKind;
      this.LineIndex = LineIndex;
   }
   AstElement[] ElementKind = new AstElement[] {};
   int[] LineIndex = new int[] {};
   public void PrintTree() {
   }
   public Tuple<AstElement, int> this[int index] => new Tuple<AstElement, int>(ElementKind[index], LineIndex[index]);
}