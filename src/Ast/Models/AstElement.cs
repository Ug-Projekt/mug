class AstElement {
   public AstElement(AstElementKind ek, Ast ast) {
      ElementKind = ek;
      ElementBody = ast;
   }
   public AstElementKind ElementKind = new AstElementKind();
   public Ast ElementBody;
   public void BuildAst(Ast ast) => ElementBody = ast;
   public static AstElement Create(AstElementKind elementKind, Ast elementBody) => new AstElement(elementKind, elementBody);
   public static AstElement Create(AstElementKind elementKind, AstBuilder elementBody) => new AstElement(elementKind, elementBody.Build());
}